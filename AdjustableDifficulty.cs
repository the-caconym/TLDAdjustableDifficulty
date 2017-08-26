/*
  AdjustableDifficulty - Fan made mod for The Long Dark.
  This is entirely fan-made and I have no affiliation to Hinterland Studios.
  License: WTFPL (Do What the Fuck You Want to Public License); you basically can do
  whatever you want with this code, with no attribution required.
*/
using System;
using System.Xml;
using Harmony;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;


// Spawn region patch.
[HarmonyPatch(typeof(SpawnRegion), "Start", new Type[0])]
class AdjustableDifficultySpawnRegionPatch
{
  // Name of the XML config file.
  static string xml_file_name="adjustable_difficulty.xml";
  // Set to 1 if XML has been read already.
  static int xml_initialized=0;
  // Defines a set of XML parameters for spawn points.
  struct xml_parameters
  {
    public float wildlife_spawns_wolves_scale_chance_active;
    public float wildlife_spawns_wolves_scale_max_respawn_per_day;
    public float wildlife_spawns_wolves_scale_max_simultaneous_spawns_during_day;
    public float wildlife_spawns_wolves_scale_max_simultaneous_spawns_during_night;
    public float wildlife_spawns_rabbits_scale_chance_active;
    public float wildlife_spawns_rabbits_scale_max_respawn_per_day;
    public float wildlife_spawns_rabbits_scale_max_simultaneous_spawns_during_day;
    public float wildlife_spawns_rabbits_scale_max_simultaneous_spawns_during_night;
    public float wildlife_spawns_bears_scale_chance_active;
    public float wildlife_spawns_bears_scale_max_respawn_per_day;
    public float wildlife_spawns_bears_scale_max_simultaneous_spawns_during_day;
    public float wildlife_spawns_bears_scale_max_simultaneous_spawns_during_night;
    public float wildlife_spawns_deers_scale_chance_active;
    public float wildlife_spawns_deers_scale_max_respawn_per_day;
    public float wildlife_spawns_deers_scale_max_simultaneous_spawns_during_day;
    public float wildlife_spawns_deers_scale_max_simultaneous_spawns_during_night;
  }
  // The parameters themselves.
  static xml_parameters[] xml_params=new xml_parameters[4];

  [HarmonyPriority(100)]
	static void Prefix(SpawnRegion __instance)
	{
		if(GameManager.IsStoryMode()) return;
		bool start_called=(bool)AccessTools.Field(typeof(SpawnRegion), "m_StartHasBeenCalled").GetValue(__instance);
		if(start_called) return;

    int exp_mode=get_current_exp_mode();
    if(exp_mode<0)
      return;

    if(xml_initialized!=1)
    {
      if(!load_xmlfile_if_not_loaded())
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to load XML file '{0}'; will do no adjustments as a result.", xml_file_name);
        return;
      }
      Debug.LogFormat("AdjustableDifficulty - Read XML configuration file '{0}' successfully.", xml_file_name);
      xml_initialized=1;
    }

    string prefab_name=__instance.m_SpawnablePrefab.name.ToLowerInvariant();
    if(prefab_name.Contains("wolf"))
    {
      adjust_spawn_point(__instance, exp_mode, "wolf", xml_params[exp_mode].wildlife_spawns_wolves_scale_chance_active,
                         xml_params[exp_mode].wildlife_spawns_wolves_scale_max_respawn_per_day,
                         xml_params[exp_mode].wildlife_spawns_wolves_scale_max_simultaneous_spawns_during_day,
                         xml_params[exp_mode].wildlife_spawns_wolves_scale_max_simultaneous_spawns_during_night);
    }
    else if(prefab_name.Contains("rabbit"))
    {
      adjust_spawn_point(__instance, exp_mode, "rabbit", xml_params[exp_mode].wildlife_spawns_rabbits_scale_chance_active,
                         xml_params[exp_mode].wildlife_spawns_rabbits_scale_max_respawn_per_day,
                         xml_params[exp_mode].wildlife_spawns_rabbits_scale_max_simultaneous_spawns_during_day,
                         xml_params[exp_mode].wildlife_spawns_rabbits_scale_max_simultaneous_spawns_during_night);
    }
    else if(prefab_name.Contains("stag"))
    {
      adjust_spawn_point(__instance, exp_mode, "deer", xml_params[exp_mode].wildlife_spawns_deers_scale_chance_active,
                         xml_params[exp_mode].wildlife_spawns_deers_scale_max_respawn_per_day,
                         xml_params[exp_mode].wildlife_spawns_deers_scale_max_simultaneous_spawns_during_day,
                         xml_params[exp_mode].wildlife_spawns_deers_scale_max_simultaneous_spawns_during_night);
    }
    else if(prefab_name.Contains("bear"))
    {
      adjust_spawn_point(__instance, exp_mode, "bear", xml_params[exp_mode].wildlife_spawns_bears_scale_chance_active,
                         xml_params[exp_mode].wildlife_spawns_bears_scale_max_respawn_per_day,
                         xml_params[exp_mode].wildlife_spawns_bears_scale_max_simultaneous_spawns_during_day,
                         xml_params[exp_mode].wildlife_spawns_bears_scale_max_simultaneous_spawns_during_night);
    }
	}


  static int get_current_exp_mode()
  {
    if(GameManager.GetExperienceModeManagerComponent().GetCurrentExperienceModeType()==ExperienceModeType.Pilgrim)
      return(0);
    else if(GameManager.GetExperienceModeManagerComponent().GetCurrentExperienceModeType()==ExperienceModeType.Voyageur)
      return(1);
    else if(GameManager.GetExperienceModeManagerComponent().GetCurrentExperienceModeType()==ExperienceModeType.Stalker)
      return(2);
    else if(GameManager.GetExperienceModeManagerComponent().GetCurrentExperienceModeType()==ExperienceModeType.Interloper)
      return(3);
    return(-1);
  }


  static void adjust_spawn_point(SpawnRegion inst, int idx, string name, float active_scale, float max_respawn_scale, float max_sim_day_scale, float max_sim_night_scale)
  {
    if((idx<0) || (idx>3))
      return;

    float previous_active=inst.m_ChanceActive;
    inst.m_ChanceActive*=active_scale;
    float previous_max_respawn=-1, new_max_respawn=-1;
    float previous_max_sim_day=-1, previous_max_sim_night=-1;
    float new_max_sim_day=-1, new_max_sim_night=-1;
    if(idx==0)
    {
      previous_max_respawn=inst.m_MaxRespawnsPerDayPilgrim;
      inst.m_MaxRespawnsPerDayPilgrim*=max_respawn_scale;
      new_max_respawn=inst.m_MaxRespawnsPerDayPilgrim;
      previous_max_sim_day=inst.m_MaxSimultaneousSpawnsDayPilgrim;
      previous_max_sim_night=inst.m_MaxSimultaneousSpawnsNightPilgrim;
      inst.m_MaxSimultaneousSpawnsDayPilgrim=Math.Max(1, (int)(inst.m_MaxSimultaneousSpawnsDayPilgrim*max_sim_day_scale));
      inst.m_MaxSimultaneousSpawnsNightPilgrim=Math.Max(1, (int)(inst.m_MaxSimultaneousSpawnsNightPilgrim*max_sim_night_scale));
      new_max_sim_day=inst.m_MaxSimultaneousSpawnsDayPilgrim;
      new_max_sim_night=inst.m_MaxSimultaneousSpawnsNightPilgrim;
    }
    else if(idx==1)
    {
      previous_max_respawn=inst.m_MaxRespawnsPerDayVoyageur;
      inst.m_MaxRespawnsPerDayVoyageur*=max_respawn_scale;
      new_max_respawn=inst.m_MaxRespawnsPerDayVoyageur;
      previous_max_sim_day=inst.m_MaxSimultaneousSpawnsDayVoyageur;
      previous_max_sim_night=inst.m_MaxSimultaneousSpawnsNightVoyageur;
      inst.m_MaxSimultaneousSpawnsDayVoyageur=Math.Max(1, (int)(inst.m_MaxSimultaneousSpawnsDayVoyageur*max_sim_day_scale));
      inst.m_MaxSimultaneousSpawnsNightVoyageur=Math.Max(1, (int)(inst.m_MaxSimultaneousSpawnsNightVoyageur*max_sim_night_scale));
      new_max_sim_day=inst.m_MaxSimultaneousSpawnsDayVoyageur;
      new_max_sim_night=inst.m_MaxSimultaneousSpawnsNightVoyageur;
    }
    else if(idx==2)
    {
      previous_max_respawn=inst.m_MaxRespawnsPerDayStalker;
      inst.m_MaxRespawnsPerDayStalker*=max_respawn_scale;
      new_max_respawn=inst.m_MaxRespawnsPerDayStalker;
      previous_max_sim_day=inst.m_MaxSimultaneousSpawnsDayStalker;
      previous_max_sim_night=inst.m_MaxSimultaneousSpawnsNightStalker;
      inst.m_MaxSimultaneousSpawnsDayStalker=Math.Max(1, (int)(inst.m_MaxSimultaneousSpawnsDayStalker*max_sim_day_scale));
      inst.m_MaxSimultaneousSpawnsNightStalker=Math.Max(1, (int)(inst.m_MaxSimultaneousSpawnsNightStalker*max_sim_night_scale));
      new_max_sim_day=inst.m_MaxSimultaneousSpawnsDayStalker;
      new_max_sim_night=inst.m_MaxSimultaneousSpawnsNightStalker;
    }
    else if(idx==3)
    {
      previous_max_respawn=inst.m_MaxRespawnsPerDayInterloper;
      inst.m_MaxRespawnsPerDayInterloper*=max_respawn_scale;
      new_max_respawn=inst.m_MaxRespawnsPerDayInterloper;
      previous_max_sim_day=inst.m_MaxSimultaneousSpawnsDayInterloper;
      previous_max_sim_night=inst.m_MaxSimultaneousSpawnsNightInterloper;
      inst.m_MaxSimultaneousSpawnsDayInterloper=Math.Max(1, (int)(inst.m_MaxSimultaneousSpawnsDayInterloper*max_sim_day_scale));
      inst.m_MaxSimultaneousSpawnsNightInterloper=Math.Max(1, (int)(inst.m_MaxSimultaneousSpawnsNightInterloper*max_sim_night_scale));
      new_max_sim_day=inst.m_MaxSimultaneousSpawnsDayInterloper;
      new_max_sim_night=inst.m_MaxSimultaneousSpawnsNightInterloper;
    }

    Debug.LogFormat("AdjustableDifficulty - Adjusted {0} spawn region: new active chance '{1:F2}' (previously '{2:F2}'), new max respawn '{3:F2}' (previously '{4:F2}'), new max respawns during day: '{5:F2}' (prev: '{6:F2}'), new max respawns during night: '{7:F2}' (prev: '{8:F2}').", name, inst.m_ChanceActive, previous_active,
                    new_max_respawn, previous_max_respawn, new_max_sim_day, previous_max_sim_day, new_max_sim_night, previous_max_sim_night);
  }


  static bool load_xmlfile_if_not_loaded()
  {
    try {
      XmlDocument xml=new XmlDocument();
      xml.Load(xml_file_name);
      // Load pilgrim stuff.
      XmlNodeList nodes=xml.DocumentElement.SelectNodes("/config/pilgrim");
      if(nodes.Count!=1)
        return(false);
      if(!load_xml_values_from_node(nodes[0], 0))
        return(false);
      // Load voyageur stuff.
      nodes=xml.DocumentElement.SelectNodes("/config/voyageur");
      if(nodes.Count!=1)
        return(false);
      if(!load_xml_values_from_node(nodes[0], 1))
        return(false);
      // Load stalker stuff.
      nodes=xml.DocumentElement.SelectNodes("/config/stalker");
      if(nodes.Count!=1)
        return(false);
      if(!load_xml_values_from_node(nodes[0], 2))
        return(false);
      // Load interloper stuff.
      nodes=xml.DocumentElement.SelectNodes("/config/interloper");
      if(nodes.Count!=1)
        return(false);
      if(!load_xml_values_from_node(nodes[0], 3))
        return(false);
    } catch(Exception e) {
      return(false);
    }
    return(true);
  }


  static bool load_xml_values_from_node(XmlNode node, int idx)
  {
    try {
      string exp_name=get_exp_mode_name(idx);

      if(!float.TryParse(node.SelectSingleNode("wildlife_spawns_wolves_scale_chance_active").Attributes["value"].Value,
                         out xml_params[idx].wildlife_spawns_wolves_scale_chance_active))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'wildlife_spawns_wolves_scale_chance_active' for experience mode {0}.",
                        exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}].", exp_name, "wildlife_spawns_wolves_scale_chance_active",
                      xml_params[idx].wildlife_spawns_wolves_scale_chance_active);

      if(!float.TryParse(node.SelectSingleNode("wildlife_spawns_wolves_scale_max_respawn_per_day").Attributes["value"].Value,
                         out xml_params[idx].wildlife_spawns_wolves_scale_max_respawn_per_day))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'wildlife_spawns_wolves_scale_max_respawn_per_day' for experience mode {0}.",
                        exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}].", exp_name, "wildlife_spawns_wolves_scale_max_respawn_per_day",
                      xml_params[idx].wildlife_spawns_wolves_scale_max_respawn_per_day);

      if(!float.TryParse(node.SelectSingleNode("wildlife_spawns_wolves_scale_max_simultaneous_spawns_during_day").Attributes["value"].Value,
                         out xml_params[idx].wildlife_spawns_wolves_scale_max_simultaneous_spawns_during_day))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'wildlife_spawns_wolves_scale_max_simultaneous_spawns_during_day' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}].", exp_name, "wildlife_spawns_wolves_scale_max_simultaneous_spawns_during_day",
                      xml_params[idx].wildlife_spawns_wolves_scale_max_simultaneous_spawns_during_day);

      if(!float.TryParse(node.SelectSingleNode("wildlife_spawns_wolves_scale_max_simultaneous_spawns_during_night").Attributes["value"].Value,
                         out xml_params[idx].wildlife_spawns_wolves_scale_max_simultaneous_spawns_during_night))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'wildlife_spawns_wolves_scale_max_simultaneous_spawns_during_night' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}].", exp_name, "wildlife_spawns_wolves_scale_max_simultaneous_spawns_during_night",
                      xml_params[idx].wildlife_spawns_wolves_scale_max_simultaneous_spawns_during_night);

      if(!float.TryParse(node.SelectSingleNode("wildlife_spawns_rabbits_scale_chance_active").Attributes["value"].Value,
                         out xml_params[idx].wildlife_spawns_rabbits_scale_chance_active))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'wildlife_spawns_rabbits_scale_chance_active' for experience mode {0}.",
                        exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}].", exp_name, "wildlife_spawns_rabbits_scale_chance_active",
                      xml_params[idx].wildlife_spawns_rabbits_scale_chance_active);

      if(!float.TryParse(node.SelectSingleNode("wildlife_spawns_rabbits_scale_max_respawn_per_day").Attributes["value"].Value,
                         out xml_params[idx].wildlife_spawns_rabbits_scale_max_respawn_per_day))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'wildlife_spawns_rabbits_scale_max_respawn_per_day' for experience mode {0}.",
                        exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}].", exp_name, "wildlife_spawns_rabbits_scale_max_respawn_per_day",
                      xml_params[idx].wildlife_spawns_rabbits_scale_max_respawn_per_day);

      if(!float.TryParse(node.SelectSingleNode("wildlife_spawns_rabbits_scale_max_simultaneous_spawns_during_day").Attributes["value"].Value,
                         out xml_params[idx].wildlife_spawns_rabbits_scale_max_simultaneous_spawns_during_day))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'wildlife_spawns_rabbits_scale_max_simultaneous_spawns_during_day' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}].", exp_name, "wildlife_spawns_rabbits_scale_max_simultaneous_spawns_during_day",
                      xml_params[idx].wildlife_spawns_rabbits_scale_max_simultaneous_spawns_during_day);

      if(!float.TryParse(node.SelectSingleNode("wildlife_spawns_rabbits_scale_max_simultaneous_spawns_during_night").Attributes["value"].Value,
                         out xml_params[idx].wildlife_spawns_rabbits_scale_max_simultaneous_spawns_during_night))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'wildlife_spawns_rabbits_scale_max_simultaneous_spawns_during_night' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}].", exp_name, "wildlife_spawns_rabbits_scale_max_simultaneous_spawns_during_night",
                      xml_params[idx].wildlife_spawns_rabbits_scale_max_simultaneous_spawns_during_night);

      if(!float.TryParse(node.SelectSingleNode("wildlife_spawns_bears_scale_chance_active").Attributes["value"].Value,
                         out xml_params[idx].wildlife_spawns_bears_scale_chance_active))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'wildlife_spawns_bears_scale_chance_active' for experience mode {0}.",
                        exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}].", exp_name, "wildlife_spawns_bears_scale_chance_active",
                      xml_params[idx].wildlife_spawns_bears_scale_chance_active);

      if(!float.TryParse(node.SelectSingleNode("wildlife_spawns_bears_scale_max_respawn_per_day").Attributes["value"].Value,
                         out xml_params[idx].wildlife_spawns_bears_scale_max_respawn_per_day))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'wildlife_spawns_bears_scale_max_respawn_per_day' for experience mode {0}.",
                        exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}].", exp_name, "wildlife_spawns_bears_scale_max_respawn_per_day",
                      xml_params[idx].wildlife_spawns_bears_scale_max_respawn_per_day);

      if(!float.TryParse(node.SelectSingleNode("wildlife_spawns_bears_scale_max_simultaneous_spawns_during_day").Attributes["value"].Value,
                         out xml_params[idx].wildlife_spawns_bears_scale_max_simultaneous_spawns_during_day))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'wildlife_spawns_bears_scale_max_simultaneous_spawns_during_day' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}].", exp_name, "wildlife_spawns_bears_scale_max_simultaneous_spawns_during_day",
                      xml_params[idx].wildlife_spawns_bears_scale_max_simultaneous_spawns_during_day);

      if(!float.TryParse(node.SelectSingleNode("wildlife_spawns_bears_scale_max_simultaneous_spawns_during_night").Attributes["value"].Value,
                         out xml_params[idx].wildlife_spawns_bears_scale_max_simultaneous_spawns_during_night))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'wildlife_spawns_bears_scale_max_simultaneous_spawns_during_night' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}].", exp_name, "wildlife_spawns_bears_scale_max_simultaneous_spawns_during_night",
                      xml_params[idx].wildlife_spawns_bears_scale_max_simultaneous_spawns_during_night);

      if(!float.TryParse(node.SelectSingleNode("wildlife_spawns_deers_scale_chance_active").Attributes["value"].Value,
                         out xml_params[idx].wildlife_spawns_deers_scale_chance_active))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'wildlife_spawns_deers_scale_chance_active' for experience mode {0}.",
                        exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}].", exp_name, "wildlife_spawns_deers_scale_chance_active",
                      xml_params[idx].wildlife_spawns_deers_scale_chance_active);

      if(!float.TryParse(node.SelectSingleNode("wildlife_spawns_deers_scale_max_respawn_per_day").Attributes["value"].Value,
                         out xml_params[idx].wildlife_spawns_deers_scale_max_respawn_per_day))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'wildlife_spawns_deers_scale_max_respawn_per_day' for experience mode {0}.",
                        exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}].", exp_name, "wildlife_spawns_deers_scale_max_respawn_per_day",
                      xml_params[idx].wildlife_spawns_deers_scale_max_respawn_per_day);

      if(!float.TryParse(node.SelectSingleNode("wildlife_spawns_deers_scale_max_simultaneous_spawns_during_day").Attributes["value"].Value,
                         out xml_params[idx].wildlife_spawns_deers_scale_max_simultaneous_spawns_during_day))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'wildlife_spawns_deers_scale_max_simultaneous_spawns_during_day' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}].", exp_name, "wildlife_spawns_deers_scale_max_simultaneous_spawns_during_day",
                      xml_params[idx].wildlife_spawns_deers_scale_max_simultaneous_spawns_during_day);

      if(!float.TryParse(node.SelectSingleNode("wildlife_spawns_deers_scale_max_simultaneous_spawns_during_night").Attributes["value"].Value,
                         out xml_params[idx].wildlife_spawns_deers_scale_max_simultaneous_spawns_during_night))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'wildlife_spawns_deers_scale_max_simultaneous_spawns_during_night' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}].", exp_name, "wildlife_spawns_deers_scale_max_simultaneous_spawns_during_night",
                      xml_params[idx].wildlife_spawns_deers_scale_max_simultaneous_spawns_during_night);
    } catch(Exception e) {
      return(false);
    }
    return(true);
  }


  static string get_exp_mode_name(int idx)
  {
    if(idx==0)
      return("pilgrim");
    else if(idx==1)
      return("voyageur");
    else if(idx==2)
      return("stalker");
    else if(idx==3)
      return("interloper");
    return("unknown");
  }
}



// Experience mode patch.
[HarmonyPatch(typeof(ExperienceMode), "Start", new Type[0])]
class AdjustableDifficultyExperienceModePatch
{
  // Name of the XML config file.
  static string xml_file_name="adjustable_difficulty.xml";
  // Set to 1 if XML has been read already.
  static int xml_initialized=0;
  // Defines a set of XML parameters for an experience mode, except for the wildlife amount stuff.
  struct xml_parameters
  {
    public int temp_drop_day_start;
    public int temp_drop_day_end;
    public float temp_drop_celsius_max;
    public float player_needs_fatigue_scale;
    public float player_needs_cold_scale;
    public float player_needs_thirst_scale;
    public float player_needs_calories_burn_scale;
    public float player_recovery_rest_scale;
    public float player_recovery_awake_scale;
    public float weather_blizzard_chances_scale;
    public float weather_current_duration_scale;
    public float objects_decay_rate_scale;
    public int objects_containers_reduce_max;
    public int objects_containers_empty_chance;
    public float damage_scale_clothing;
    public float damage_scale_bear_attack;
    public float damage_scale_wolf_attack;
    public float wolf_struggle_defense_strength_scale;
    public float predators_smell_distance_scale;
    public int respawn_delay_scale_day_start;
    public int respawn_delay_scale_day_end;
    public float respawn_delay_scale_max;
    public float hypothermia_cure_delay_scale;
    public int fishing_catch_scale_day_start;
    public int fishing_catch_scale_day_end;
    public float fishing_catch_scale_max;
  }
  // The parameters themselves.
  static xml_parameters[] xml_params=new xml_parameters[4];


  // Main patch function.
  [HarmonyPriority(100)]
  static void Postfix(ExperienceMode __instance)
  {
    if(xml_initialized!=1)
    {
      if(!load_xmlfile_if_not_loaded())
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to load XML file '{0}'; will do no adjustments as a result.", xml_file_name);
        return;
      }
      Debug.LogFormat("AdjustableDifficulty - Read XML configuration file '{0}' successfully.", xml_file_name);
      xml_initialized=1;
    }

    if(__instance.m_ModeType==ExperienceModeType.Pilgrim)   // Pilgrim.
      patch_values(__instance, 0);
    else if(__instance.m_ModeType==ExperienceModeType.Voyageur)   // Voyageur.
      patch_values(__instance, 1);
    else if(__instance.m_ModeType==ExperienceModeType.Stalker)    // Stalker.
      patch_values(__instance, 2);
    else if(__instance.m_ModeType==ExperienceModeType.Interloper)    // Interloper.
      patch_values(__instance, 3);
  }


  static void patch_values(ExperienceMode inst, int idx)
  {
    inst.m_OutdoorTempDropDayStart=xml_params[idx].temp_drop_day_start;
    inst.m_OutdoorTempDropDayFinal=xml_params[idx].temp_drop_day_end;
    inst.m_OutdoorTempDropCelsiusMax=xml_params[idx].temp_drop_celsius_max;
    inst.m_FatigueRateScale=xml_params[idx].player_needs_fatigue_scale;
    inst.m_FreezingRateScale=xml_params[idx].player_needs_cold_scale;
    inst.m_ThirstRateScale=xml_params[idx].player_needs_thirst_scale;
    inst.m_CalorieBurnScale=xml_params[idx].player_needs_calories_burn_scale;
    inst.m_ConditonRecoveryFromRestScale=xml_params[idx].player_recovery_rest_scale;
    inst.m_ConditonRecoveryWhileAwakeScale=xml_params[idx].player_recovery_awake_scale;
    inst.m_ChanceOfBlizzardScale=xml_params[idx].weather_blizzard_chances_scale;
    inst.m_WeatherDurationScale=xml_params[idx].weather_current_duration_scale;
    inst.m_DecayScale=xml_params[idx].objects_decay_rate_scale;
    inst.m_ReduceMaxItemsInContainer=xml_params[idx].objects_containers_reduce_max;
    inst.m_ChanceForEmptyContainer=xml_params[idx].objects_containers_empty_chance;
    inst.m_StrugglePlayerClothingDamageScale=xml_params[idx].damage_scale_clothing;
    inst.m_StugglePlayerPercentLossFromBearScale=xml_params[idx].damage_scale_bear_attack;
    inst.m_StrugglePlayerDamageReceivedScale=xml_params[idx].damage_scale_wolf_attack;
    inst.m_StruggleTapStrengthScale=xml_params[idx].wolf_struggle_defense_strength_scale;
    inst.m_SmellRangeScale=xml_params[idx].predators_smell_distance_scale;
    inst.m_RespawnHoursScaleDayStart=xml_params[idx].respawn_delay_scale_day_start;
    inst.m_RespawnHoursScaleDayFinal=xml_params[idx].respawn_delay_scale_day_end;
    inst.m_RespawnHoursScaleMax=xml_params[idx].respawn_delay_scale_max;
    inst.m_NumHoursWarmForHypothermiaCureScale=xml_params[idx].hypothermia_cure_delay_scale;
    inst.m_FishCatchTimeScaleDayStart=xml_params[idx].fishing_catch_scale_day_start;
    inst.m_FishCatchTimeScaleDayFinal=xml_params[idx].fishing_catch_scale_day_end;
    inst.m_FishCatchTimeScaleMax=xml_params[idx].fishing_catch_scale_max;

    Debug.LogFormat("AdjustableDifficulty - Patched 26 values successfully for experience mode '{0}'.", get_exp_mode_name(idx));
  }


  static bool load_xmlfile_if_not_loaded()
  {
    try {
      XmlDocument xml=new XmlDocument();
      xml.Load(xml_file_name);
      // Load pilgrim stuff.
      XmlNodeList nodes=xml.DocumentElement.SelectNodes("/config/pilgrim");
      if(nodes.Count!=1)
        return(false);
      if(!load_xml_values_from_node(nodes[0], 0))
        return(false);
      // Load voyageur stuff.
      nodes=xml.DocumentElement.SelectNodes("/config/voyageur");
      if(nodes.Count!=1)
        return(false);
      if(!load_xml_values_from_node(nodes[0], 1))
        return(false);
      // Load stalker stuff.
      nodes=xml.DocumentElement.SelectNodes("/config/stalker");
      if(nodes.Count!=1)
        return(false);
      if(!load_xml_values_from_node(nodes[0], 2))
        return(false);
      // Load interloper stuff.
      nodes=xml.DocumentElement.SelectNodes("/config/interloper");
      if(nodes.Count!=1)
        return(false);
      if(!load_xml_values_from_node(nodes[0], 3))
        return(false);
    } catch(Exception e) {
      return(false);
    }
    return(true);
  }


  static bool load_xml_values_from_node(XmlNode node, int idx)
  {
    try {
      string exp_name=get_exp_mode_name(idx);

      if(!int.TryParse(node.SelectSingleNode("temp_drop_day_start").Attributes["value"].Value, out xml_params[idx].temp_drop_day_start))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read int value of parameter 'temp_drop_day_start' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:D}]", exp_name, "temp_drop_day_start",
                      xml_params[idx].temp_drop_day_start);

      if(!int.TryParse(node.SelectSingleNode("temp_drop_day_end").Attributes["value"].Value, out xml_params[idx].temp_drop_day_end))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read int value of parameter 'temp_drop_day_end' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:D}]", exp_name, "temp_drop_day_end",
                      xml_params[idx].temp_drop_day_end);

      if(!float.TryParse(node.SelectSingleNode("temp_drop_celsius_max").Attributes["value"].Value, out xml_params[idx].temp_drop_celsius_max))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'temp_drop_celsius_max' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}]", exp_name, "temp_drop_celsius_max",
                      xml_params[idx].temp_drop_celsius_max);

      if(!float.TryParse(node.SelectSingleNode("player_needs_fatigue_scale").Attributes["value"].Value, out xml_params[idx].player_needs_fatigue_scale))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'player_needs_fatigue_scale' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}]", exp_name, "player_needs_fatigue_scale",
                      xml_params[idx].player_needs_fatigue_scale);

      if(!float.TryParse(node.SelectSingleNode("player_needs_cold_scale").Attributes["value"].Value, out xml_params[idx].player_needs_cold_scale))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'player_needs_cold_scale' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}]", exp_name, "player_needs_cold_scale",
                      xml_params[idx].player_needs_cold_scale);

      if(!float.TryParse(node.SelectSingleNode("player_needs_thirst_scale").Attributes["value"].Value, out xml_params[idx].player_needs_thirst_scale))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'player_needs_thirst_scale' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}]", exp_name, "player_needs_thirst_scale",
                      xml_params[idx].player_needs_thirst_scale);

      if(!float.TryParse(node.SelectSingleNode("player_needs_calories_burn_scale").Attributes["value"].Value, out xml_params[idx].player_needs_calories_burn_scale))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'player_needs_calories_burn_scale' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}]", exp_name, "player_needs_calories_burn_scale",
                      xml_params[idx].player_needs_calories_burn_scale);

      if(!float.TryParse(node.SelectSingleNode("player_recovery_rest_scale").Attributes["value"].Value, out xml_params[idx].player_recovery_rest_scale))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'player_recovery_rest_scale' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}]", exp_name, "player_recovery_rest_scale",
                      xml_params[idx].player_recovery_rest_scale);

      if(!float.TryParse(node.SelectSingleNode("player_recovery_awake_scale").Attributes["value"].Value, out xml_params[idx].player_recovery_awake_scale))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'player_recovery_awake_scale' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}]", exp_name, "player_recovery_awake_scale",
                      xml_params[idx].player_recovery_awake_scale);

      if(!float.TryParse(node.SelectSingleNode("weather_blizzard_chances_scale").Attributes["value"].Value, out xml_params[idx].weather_blizzard_chances_scale))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'weather_blizzard_chances_scale' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}]", exp_name, "weather_blizzard_chances_scale",
                      xml_params[idx].weather_blizzard_chances_scale);

      if(!float.TryParse(node.SelectSingleNode("weather_current_duration_scale").Attributes["value"].Value, out xml_params[idx].weather_current_duration_scale))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'weather_current_duration_scale' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}]", exp_name, "weather_current_duration_scale",
                      xml_params[idx].weather_current_duration_scale);

      if(!float.TryParse(node.SelectSingleNode("objects_decay_rate_scale").Attributes["value"].Value, out xml_params[idx].objects_decay_rate_scale))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'objects_decay_rate_scale' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}]", exp_name, "objects_decay_rate_scale",
                      xml_params[idx].objects_decay_rate_scale);

      if(!int.TryParse(node.SelectSingleNode("objects_containers_reduce_max").Attributes["value"].Value, out xml_params[idx].objects_containers_reduce_max))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read int value of parameter 'objects_containers_reduce_max' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:D}]", exp_name, "objects_containers_reduce_max",
                      xml_params[idx].objects_containers_reduce_max);

      if(!int.TryParse(node.SelectSingleNode("objects_containers_empty_chance").Attributes["value"].Value, out xml_params[idx].objects_containers_empty_chance))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read int value of parameter 'objects_containers_empty_chance' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:D}]", exp_name, "objects_containers_empty_chance",
                      xml_params[idx].objects_containers_empty_chance);

      if(!float.TryParse(node.SelectSingleNode("damage_scale_clothing").Attributes["value"].Value, out xml_params[idx].damage_scale_clothing))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'damage_scale_clothing' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}]", exp_name, "damage_scale_clothing",
                      xml_params[idx].damage_scale_clothing);

      if(!float.TryParse(node.SelectSingleNode("damage_scale_bear_attack").Attributes["value"].Value, out xml_params[idx].damage_scale_bear_attack))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'damage_scale_bear_attack' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}]", exp_name, "damage_scale_bear_attack",
                      xml_params[idx].damage_scale_bear_attack);

      if(!float.TryParse(node.SelectSingleNode("damage_scale_wolf_attack").Attributes["value"].Value, out xml_params[idx].damage_scale_wolf_attack))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'damage_scale_wolf_attack' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}]", exp_name, "damage_scale_wolf_attack",
                      xml_params[idx].damage_scale_wolf_attack);

      if(!float.TryParse(node.SelectSingleNode("wolf_struggle_defense_strength_scale").Attributes["value"].Value, out xml_params[idx].wolf_struggle_defense_strength_scale))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'wolf_struggle_defense_strength_scale' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}]", exp_name, "wolf_struggle_defense_strength_scale",
                      xml_params[idx].wolf_struggle_defense_strength_scale);

      if(!float.TryParse(node.SelectSingleNode("predators_smell_distance_scale").Attributes["value"].Value, out xml_params[idx].predators_smell_distance_scale))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'predators_smell_distance_scale' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}]", exp_name, "predators_smell_distance_scale",
                      xml_params[idx].predators_smell_distance_scale);

      if(!int.TryParse(node.SelectSingleNode("respawn_delay_scale_day_start").Attributes["value"].Value, out xml_params[idx].respawn_delay_scale_day_start))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read int value of parameter 'respawn_delay_scale_day_start' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:D}]", exp_name, "respawn_delay_scale_day_start",
                      xml_params[idx].respawn_delay_scale_day_start);

      if(!int.TryParse(node.SelectSingleNode("respawn_delay_scale_day_end").Attributes["value"].Value, out xml_params[idx].respawn_delay_scale_day_end))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read int value of parameter 'respawn_delay_scale_day_end' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:D}]", exp_name, "respawn_delay_scale_day_end",
                      xml_params[idx].respawn_delay_scale_day_end);

      if(!float.TryParse(node.SelectSingleNode("respawn_delay_scale_max").Attributes["value"].Value, out xml_params[idx].respawn_delay_scale_max))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'respawn_delay_scale_max' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}]", exp_name, "respawn_delay_scale_max",
                      xml_params[idx].respawn_delay_scale_max);

      if(!float.TryParse(node.SelectSingleNode("hypothermia_cure_delay_scale").Attributes["value"].Value, out xml_params[idx].hypothermia_cure_delay_scale))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'hypothermia_cure_delay_scale' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}]", exp_name, "hypothermia_cure_delay_scale",
                      xml_params[idx].hypothermia_cure_delay_scale);

      if(!int.TryParse(node.SelectSingleNode("fishing_catch_scale_day_start").Attributes["value"].Value, out xml_params[idx].fishing_catch_scale_day_start))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read int value of parameter 'fishing_catch_scale_day_start' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:D}]", exp_name, "fishing_catch_scale_day_start",
                      xml_params[idx].fishing_catch_scale_day_start);

      if(!int.TryParse(node.SelectSingleNode("fishing_catch_scale_day_end").Attributes["value"].Value, out xml_params[idx].fishing_catch_scale_day_end))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read int value of parameter 'fishing_catch_scale_day_end' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:D}]", exp_name, "fishing_catch_scale_day_end",
                      xml_params[idx].fishing_catch_scale_day_end);

      if(!float.TryParse(node.SelectSingleNode("fishing_catch_scale_max").Attributes["value"].Value, out xml_params[idx].fishing_catch_scale_max))
      {
        Debug.LogFormat("AdjustableDifficulty - ERROR: unable to read float value of parameter 'fishing_catch_scale_max' for experience mode {0}.", exp_name);
        return(false);
      }
      Debug.LogFormat("AdjustableDifficulty - Read value for {0}/{1} successfully: [{2:F}]", exp_name, "fishing_catch_scale_max",
                      xml_params[idx].fishing_catch_scale_max);
    } catch(Exception e) {
      return(false);
    }
    return(true);
  }


  static string get_exp_mode_name(int idx)
  {
    if(idx==0)
      return("pilgrim");
    else if(idx==1)
      return("voyageur");
    else if(idx==2)
      return("stalker");
    else if(idx==3)
      return("interloper");
    return("unknown");
  }
}
