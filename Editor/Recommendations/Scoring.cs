using System;
using System.Collections.Generic;
using System.Linq;

namespace Unity.Multiplayer.Center.Recommendations
{
    struct ScoreWithReason
    {
        public float Score;
        public string Reason;

        public ScoreWithReason(float score, string reason)
        {
            Score = score;
            Reason = reason;
        }
    }

    /// <summary>
    /// Aggregates scores for a given solution and fetches the reasons for the highest score
    /// </summary>
    internal class Scoring
    {
        List<ScoreWithReason> m_AllScores = new();
        public float TotalScore { get; private set; } = 0f;

        public static Scoring CreateFromUniqueScore(float score, string reason)
        {
            var scoring = new Scoring();
            scoring.AddScore(score, reason);
            return scoring;
        }
        
        public void AddScore(float score, string reason)
        {
            TotalScore += score;
            if(m_AllScores.Count == 0)
            {
                m_AllScores.Add(new ScoreWithReason(score, reason));
                return;
            }

            int insertIndex = 0;
            for (int i = 0; i < m_AllScores.Count; i++)
            {
                if (score > m_AllScores[i].Score)
                {
                    insertIndex = i;
                    break;
                }
            }
            
            m_AllScores.Insert(insertIndex, new ScoreWithReason(score, reason));
        }

        /// <summary>
        /// Gets the reason for increased scores
        /// </summary>
        /// <returns>The explanatory string</returns>
        public string GetReasonString() => GetAllContributionsReasons();
        
        string GetAllContributionsReasons()
        {
            return String.Join("\n", m_AllScores.Select(s => s.Reason));
        }

        // Only max contribution
        string GetMaxContributionReason()
        {
            return m_AllScores.Count == 0 ? null : m_AllScores[0].Reason;
        }
    }
}
