using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour
{
    [SerializeField]
    SingleTeamClassification m_Team = new SingleTeamClassification();

    [SerializeField]
    protected List<TeamClassification> friendlyTeams = new List<TeamClassification>();

    [SerializeField]
    protected List<TeamClassification> enemyTeams = new List<TeamClassification>();

    public static TeamClassification GetTeam(TeamTag tag)
    {
        TeamClassification classification = new TeamClassification();
        classification.Team = tag;
        classification.Types = new TeamTypeTag[] { TeamTypeTag.All };

        return classification;
    }

    public bool IsFriendly(Team team)
    {
        return IsFriendly(team.CurrentTeam);
    }
    public bool IsFriendly(SingleTeamClassification teamClassification)
    {
        return IsFriendly(FriendlyTeams, teamClassification);
    }
    public static bool IsFriendly(List<TeamClassification> friendlyClassifications, SingleTeamClassification teamClassification)
    {
        for (int i = 0; i < friendlyClassifications.Count; i++)
        {
            if (friendlyClassifications[i].Team == TeamTag.All)
                return true;

            if (friendlyClassifications[i].Team != teamClassification.Team)
                continue;

            for (int k = 0; i < friendlyClassifications[i].Types.Length; k++)
            {
                if (friendlyClassifications[i].Types[k] == TeamTypeTag.All)
                    return true;

                if (friendlyClassifications[i].Types[k] != teamClassification.Type)
                    continue;

                return true;
            }
        }

        return false;
    }
    public bool IsEnemy(Team team)
    {
        return IsEnemy(team.CurrentTeam);
    }
    public bool IsEnemy(SingleTeamClassification teamClassification)
    {    
        for (int i = 0; i < EnemyTeams.Count; i++)
        {
            if (EnemyTeams[i].Team == TeamTag.All)
                return true;

            if (EnemyTeams[i].Team != teamClassification.Team)
                continue;

            for (int k = 0; k < EnemyTeams[i].Types.Length; k++)
            {
                if (EnemyTeams[i].Types[k] == TeamTypeTag.All)
                    return true;

                if (EnemyTeams[i].Types[k] != teamClassification.Type)
                    continue;

                return true;
            }
        }

        return false;
    }

    #region Accessors

    public TeamTag CurrentTeamTag
    {
        get { return m_Team.Team; }
        set { m_Team.Team = value; }
    }
    public SingleTeamClassification CurrentTeam
    {
        get { return m_Team; }
        private set { m_Team = value; }
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
public class SingleTeamClassification
{
    [SerializeField]
    private TeamTag m_Team;

    [SerializeField]
    private TeamTypeTag m_Type;

    public TeamTag Team
    {
        get { return m_Team; }
        set { m_Team = value; }
    }
    public TeamTypeTag Type
    {
        get { return m_Type; }
        set { m_Type = value; }
    }
}
[System.Serializable]
public class TeamClassification
{
    [SerializeField]
    private TeamTag m_Team;

    [SerializeField]
    private TeamTypeTag[] m_Types;

    public TeamTag Team
    {
        get { return m_Team; }
        set { m_Team = value; }
    }
    public TeamTypeTag[] Types
    {
        get { return m_Types; }
        set { m_Types = value; }
    }
}
