using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour
{
    public enum Type
    {
        All,
        Player,
        Wild
    }

    [SerializeField]
    Type m_TeamType;
    
    [SerializeField]
    protected List<TeamClassification> friendlyTeams = new List<TeamClassification>();

    [SerializeField]
    protected List<TeamClassification> enemyTeams = new List<TeamClassification>();

    Identifier m_Identifier;

    protected virtual void Awake()
    {
        m_Identifier = GetComponent<Identifier>();
    }

    public static TeamClassification GetTeam(Team.Type type)
    {
        TeamClassification classification = new TeamClassification(type, new string[] { "ALL" });

        return classification;
    }
    public static bool IsClassified(Team.Type type, string tag, List<TeamClassification> classifications)
    {
        for (int i = 0; i < classifications.Count; i++)
        {
            if (classifications[i].TeamType == Team.Type.All)
                return true;

            if (type != classifications[i].TeamType)
                continue;

            for (int k = 0; k < classifications[i].TeamTags.Length; k++)
            {
                if (classifications[i].TeamTags[k].Equals("ALL"))
                    return true;

                if (!classifications[i].TeamTags[k].Equals(tag.ToUpper()))
                    continue;

                return true;
            }
        }

        return false;
    }

    public bool IsFriendly(Team team)
    {
        return IsFriendly(team.TeamType, team.TeamTag);
    }
    public bool IsFriendly(Team.Type type, string tag)
    {
        return IsClassified(type, tag, FriendlyTeams);
    }
    
    public bool IsEnemy(Team team)
    {
        return IsEnemy(team.TeamType, team.TeamTag);
    }
    public bool IsEnemy(Team.Type type, string tag)
    {
        return IsClassified(type, tag, EnemyTeams);
    }

    public void Copy(Team team)
    {
        if (team == null)
        {
            return;
        }

 
        TeamType = team.TeamType;
        FriendlyTeams = team.FriendlyTeams;
        FriendlyTeams.Add(Team.GetTeam(TeamType));
        EnemyTeams = team.EnemyTeams;
    }

    public void ResetTeam()
    {
        TeamType = Team.Type.All;
        FriendlyTeams.Clear();
        EnemyTeams.Clear();
    }

    #region Accessors

    public Team.Type TeamType
    {
        get { return m_TeamType; }
        set { m_TeamType = value; }
    }
    public string TeamTag
    {
        get
        {
            return m_Identifier.Name;
        }
    }

    public List<TeamClassification> FriendlyTeams
    {
        get { return friendlyTeams; }
        set { friendlyTeams = value; }
    }
    public List<TeamClassification> EnemyTeams
    {
        get { return enemyTeams; }
        set { enemyTeams = value; }
    }

    #endregion
}


[System.Serializable]
public class TeamClassification
{
    [SerializeField]
    Team.Type m_TeamType;

    [SerializeField]
    string[] m_TeamTags;

    public TeamClassification(Team.Type type, string[] tags)
    {
        TeamType = type;
        TeamTags = tags;
    }

    public Team.Type TeamType
    {
        get { return m_TeamType; }
        set { m_TeamType = value; }
    }
    public string[] TeamTags
    {
        get { return m_TeamTags; }
        set { m_TeamTags = value; }
    }
}
