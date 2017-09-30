using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ITeamMember
{
    Team GetTeam();

    SingleTeamClassification GetCurrentTeam();
    TeamClassification[] GetFriendlyTeams();
    TeamClassification[] GetEnemyTeams();

    Transform Transform { get; }
}

[System.Serializable]
public class Team
{
    [SerializeField]
    SingleTeamClassification m_Team;

    [SerializeField]
    protected TeamClassification[] friendlyTeams;

    [SerializeField]
    protected TeamClassification[] enemyTeams;


    public void SetCurrentTeamTag(TeamTag tag)
    {
        m_Team.Team = tag;
    }
    public void SetFriendlyTeams(TeamClassification[] _teams)
    {
        FriendlyTeams = _teams;
    }
    public void SetEnemyTeams(TeamClassification[] _teams)
    {
        EnemyTeams = _teams;
    }

    #region Accessors

    public SingleTeamClassification CurrentTeam
    {
        get { return m_Team; }
        private set { m_Team = value; }
    }

    public TeamClassification[] FriendlyTeams
    {
        get { return friendlyTeams; }
        private set { friendlyTeams = value; }
    }
    public TeamClassification[] EnemyTeams
    {
        get { return enemyTeams; }
        private set { enemyTeams = value; }
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


public enum TeamTag
{
    All,
    Player,
    Wild
}

public enum TeamTypeTag
{
    All,
    Player,
    Forge,
    Reaper
      
}


public static class TeamUtility
{
    public static bool IsFriendly(ITeamMember teamMember, SingleTeamClassification teamClassification)
    {
        return IsFriendly(teamMember.GetFriendlyTeams(), teamClassification);
    }
    public static bool IsFriendly(TeamClassification[] friendlyClassifications, SingleTeamClassification teamClassification)
    {
        for(int i = 0; i < friendlyClassifications.Length; i++)
        {
            if (friendlyClassifications[i].Team == TeamTag.All)
                return true;

            if (friendlyClassifications[i].Team != teamClassification.Team)
                continue;

            for(int k = 0; i < friendlyClassifications[i].Types.Length; k++)
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
    public static bool IsEnemy(ITeamMember teamMember, SingleTeamClassification teamClassification)
    {
        TeamClassification[] enemyClassifications = teamMember.GetEnemyTeams();

        for (int i = 0; i < enemyClassifications.Length; i++)
        {
            if (enemyClassifications[i].Team == TeamTag.All)
                return true;

            if (enemyClassifications[i].Team != teamClassification.Team)
                continue;

            for (int k = 0; k < enemyClassifications[i].Types.Length; k++)
            {
                if (enemyClassifications[i].Types[k] == TeamTypeTag.All)
                    return true;

                if (enemyClassifications[i].Types[k] != teamClassification.Type)
                    continue;

                return true;
            }
        }

        return false;
    }
}
