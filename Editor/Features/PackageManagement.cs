using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Unity.Multiplayer.Center
{
    internal static class PackageManagement
    {
        static PackageInstaller s_Installer;
        
        /// <summary>
        /// Opens the package manager window and hides error
        /// </summary>
        public static void OpenPackageManager()
        {
            try
            {
                UnityEditor.PackageManager.UI.Window.Open("UnityRegistry");
            }
            catch (Exception)
            {
                // Hide the error in the PackageManager API until the team fixes it
                // Debug.Log("Error opening Package Manager: " + e.Message);
            }
        }

        /// <summary>
        /// Opens the package manager window with selected package name and hides error
        /// </summary>
        public static void OpenPackageManager(string packageName)
        {
            try
            {
                UnityEditor.PackageManager.UI.Window.Open(packageName);
            }
            catch (Exception)
            {
                // Hide the error in the PackageManager API until the team fixes it
                // Debug.Log("Error opening Package Manager: " + e.Message);
            }
        }

        /// <summary>
        /// Checks if the package is a direct dependency of the project
        /// </summary>
        /// <param name="packageId">The package name/id e.g. com.unity.netcode</param>
        /// <returns>True if the package is a direct dependency</returns>
        public static bool IsDirectDependency(string packageId)
        {
            var package = GetInstalledPackage(packageId);
            return package != null && package.isDirectDependency;
        }
        
        public static bool IsInstalled(string packageId) => GetInstalledPackage(packageId) != null;

        /// <summary>
        /// Finds the installed package with the given packageId or returns null.
        /// </summary>
        /// <param name="packageId">The package name/id e.g. com.unity.netcode</param>
        /// <returns>The package info</returns>
        public static UnityEditor.PackageManager.PackageInfo GetInstalledPackage(string packageId)
        {
            return UnityEditor.PackageManager.PackageInfo.FindForPackageName(packageId);
        }

        /// <summary>
        /// Returns true if any of the given packageIds is installed.
        /// </summary>
        /// <param name="packageIds">List of package is e.g com.unity.netcode</param>
        /// <returns>True if any package is installed, false otherwise</returns>
        public static bool IsAnyPackageInstalled(params string[] packageIds)
        {
            var installedPackages = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();
            var hashset = new HashSet<string>(installedPackages.Select(e => e.name));
            return packageIds.Any(hashset.Contains);
        }

        /// <summary>
        /// Installs a single package and invokes the callback when the package is installed/when the install failed.
        /// </summary>
        /// <param name="packageId">The package name/id e.g. com.unity.netcode</param>
        /// <param name="onInstalled">The callback</param>
        public static void InstallPackage(string packageId, Action<bool> onInstalled = null)
        {
            s_Installer = new PackageInstaller(packageId);
            s_Installer.OnInstalled += onInstalled;
            s_Installer.OnInstalled += _ => s_Installer = null;
        }

        /// <summary>
        /// Installs several packages and invokes the callback when all packages are installed/when the install failed.
        /// </summary>
        /// <param name="packageIds">The package names/ids e.g. com.unity.netcode</param>
        /// <param name="onAllInstalled">The callback</param>
        /// <param name="packageIdsToRemove">Optional package name/ids to remove</param>
        public static void InstallPackages(IEnumerable<string> packageIds, Action<bool> onAllInstalled = null, IEnumerable<string> packageIdsToRemove = null)
        {
            s_Installer = new PackageInstaller(packageIds, packageIdsToRemove);
            s_Installer.OnInstalled += onAllInstalled;
            s_Installer.OnInstalled += _ => s_Installer = null;
        }

        /// <summary>
        /// Create a dictionary with package names as keys and versions as values    
        /// </summary>
        /// <returns></returns>
        internal static Dictionary<string, string> InstalledPackageDictionary()
        {
            var installedPackages = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();

            var installedPackageDictionary = installedPackages
                .Select(e => e.packageId.Split('@'))
                .Where(e => e.Length == 2)
                .ToDictionary(e => e[0], e => e[1]);

            return installedPackageDictionary;
        }

        class PackageInstaller
        {
            Request m_Request;
            string[] m_PackagesToAddIds = new string[0];
            public event Action<bool> OnInstalled;

            public PackageInstaller(string packageId)
            {
                // Add a package to the project
                m_Request = Client.Add(packageId);
                m_PackagesToAddIds = new[] {packageId};
                EditorApplication.update += Progress;
            }

            public PackageInstaller(IEnumerable<string> packageIds, IEnumerable<string> packageIdsToRemove = null)
            {
                // Add a package to the project
                m_Request = Client.AddAndRemove(packageIds.ToArray(), packageIdsToRemove?.ToArray());
                m_PackagesToAddIds = packageIds.ToArray();
                EditorApplication.update += Progress;
            }

            public bool IsInstallationFinished()
            {
                return m_Request == null || m_Request.IsCompleted;
            }
            void Progress()
            {
                if (!m_Request.IsCompleted) return;

                if (m_Request.Status == StatusCode.Success)
                    Debug.Log("Installed: " + GetInstalledPackageId());
                else if (m_Request.Status >= StatusCode.Failure)
                {
                    // if the request has more than one package, it will only prompt error message for one  
                    // We should prompt all the failed packages
                    Debug.Log("Package installation request with selected packages: " + String.Join(", ", m_PackagesToAddIds) +
                        " failed. \n Reason: "+ m_Request.Error.message);
                    
                    if (m_Request.Error.message.Contains("com.unity.multiplayer.widgets"))
                    {
                        Debug.Log("The package is only available in the internal registry. Please make sure you are connected to the company network via VPN.");
                    }
                }

                EditorApplication.update -= Progress;
                OnInstalled?.Invoke(m_Request.Status == StatusCode.Success);
            }

            string GetInstalledPackageId()
            {
                return m_Request switch
                {
                    AddRequest addRequest => addRequest.Result.packageId,
                    AddAndRemoveRequest addAndRemoveRequest => string.Join(", ", addAndRemoveRequest.Result.Select(e => e.packageId)),
                    _ => throw new InvalidOperationException("Unknown request type")
                };
            }
        }

        /// <summary>
        /// Detects if any multiplayer package is installed by checking for services and Netcode installed packages.
        /// </summary>
        /// <returns>True if any package was detected, False otherwise</returns>
        public static bool IsAnyMultiplayerPackageInstalled()
        {
            var packagesToCheck = new string[]
            {
                "com.unity.netcode",
                "com.unity.netcode.gameobjects",
                "com.unity.services.core", // Installed with all package services
                "com.unity.services.multiplayer"
            };

            return packagesToCheck.Any(IsInstalled);
        }
        
        /// <summary>
        /// Checks if the installation process has finished.
        /// </summary>
        /// <returns>True if there is no current installer instance or installation is finished on the installer</returns>
        public static bool IsInstallationFinished()
        {
            return s_Installer == null || s_Installer.IsInstallationFinished();
        }
    }
}
