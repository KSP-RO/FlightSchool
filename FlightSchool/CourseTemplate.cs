﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MagiCore;

namespace FlightSchool
{
    public enum ExpireMode {IMMEDIATE, RECOVERY}; //expire as soon as time runs out, or on recovery
    public enum RepeatMode {NEVER, EXPIRED, ALWAYS}; //is the course repeatable?

    public class Reward
    {
        string CourseID = "";
        int XP = 0;
        ConfigNode FlightLog = new ConfigNode();

        ConfigNode MiscRewards = new ConfigNode();

        ConfigNode[] Expiry = { };
    }

    public class Expiry
    {
        public ExpireMode expireType = ExpireMode.IMMEDIATE;
        public double timeOut = 0;
    }

    public class CourseTemplate
    {
        public ConfigNode sourceNode = new ConfigNode();

        public Dictionary<string, string> Variables = new Dictionary<string, string>();

        public string id = "";
        public string name = "";
        public string description = "";

        public bool Available = true; //Whether the course is currently being offered

        public string[] activePreReqs = { }; //prereqs that must not be expired
        public string[] preReqs = { }; //prereqs that must be taken, but can be expired
        public string[] conflicts = { }; //course IDs that cannot be taken while this course is not expired

        public double time = 0; //how much time the course takes (in seconds)
        public bool required = false; //whether the course is required for the kerbal to be usable
        public RepeatMode repeatable = RepeatMode.NEVER; //whether the course can be repeated

        public string[] classes = { }; //which classes can take this course (empty == all)
        public int minLevel = 0; //minimum kerbal level required to take the course
        public int maxLevel = 5; //max kerbal level allowed to take the course

        public int seatMax = -1; //maximum number of kerbals allowed in the course at once
        public int seatMin = 0; //minimum number of kerbals required to start the course

        public double costBase = 0; //base cost of the class
        public double costSeat = 0; //cost per seat

        public double costTeacher = 0; //cost of hiring a teacher rather than using one of our own kerbals
        public string[] teachClasses = { }; //which classes are valid teachers (empty == all)
        public int teachMinLevel = 0; //minimum level for the teacher

        public int rewardXP = 0; //pure XP reward
        public ConfigNode RewardLog = null; //the flight log to insert
       // public ConfigNode[] Expiry = { }; //The list of ways that course experience can be lost

        public CourseTemplate(ConfigNode source)
        {
            sourceNode = source;
        }

        public CourseTemplate(ConfigNode source, bool copy)
        {
            if (copy)
                sourceNode = source.CreateCopy();
            else
                sourceNode = source;
        }

        public CourseTemplate()
        {

        }

        /// <summary>
        /// Populates all fields from the provided ConfigNode, parsing all formulas
        /// </summary>
        /// <param name="variables"></param>
        public void PopulateFromSourceNode(Dictionary<string, string> variables, ConfigNode source = null)
        {
            if (source == null)
                source = sourceNode;

            Variables = variables;

            id = MathParsing.ReplaceMathVariables("FlightSchool", ConfigNodeExtensions.GetValueOrDefault(source, "id"), variables);
            name = MathParsing.ReplaceMathVariables("FlightSchool", ConfigNodeExtensions.GetValueOrDefault(source, "name"), variables);
            description = MathParsing.ReplaceMathVariables("FlightSchool", ConfigNodeExtensions.GetValueOrDefault(source, "description"), variables);

            bool.TryParse(MathParsing.ReplaceMathVariables("FlightSchool", ConfigNodeExtensions.GetValueOrDefault(source, "Available", "True"), variables), out Available);

            List<string> tmpList = MathParsing.ReplaceMathVariables("FlightSchool", ConfigNodeExtensions.GetValueOrDefault(source, "activePreReqs"), variables).Split(',').ToList();
            tmpList.ForEach((s) => s.Trim());
            if (!tmpList.TrueForAll(s => s == ""))
                activePreReqs = tmpList.ToArray();
            else
                activePreReqs = new string[] { };
            //activePreReqs = tmpList.ToArray();

            tmpList = MathParsing.ReplaceMathVariables("FlightSchool", ConfigNodeExtensions.GetValueOrDefault(source, "preReqs"), variables).Split(',').ToList();
            tmpList.ForEach((s) => s.Trim());
            if (!tmpList.TrueForAll(s => s == ""))
                preReqs = tmpList.ToArray();
            else
                preReqs = new string[] { };

            tmpList = MathParsing.ReplaceMathVariables("FlightSchool", ConfigNodeExtensions.GetValueOrDefault(source, "conflicts"), variables).Split(',').ToList();
            tmpList.ForEach((s) => s.Trim());
            //conflicts = tmpList.ToArray();
            if (!tmpList.TrueForAll(s => s == ""))
                conflicts = tmpList.ToArray();
            else
                conflicts = new string[] { };

            time = MathParsing.ParseMath("FlightSchool", ConfigNodeExtensions.GetValueOrDefault(source, "time", "0"), variables);
            bool.TryParse(MathParsing.ReplaceMathVariables("FlightSchool", ConfigNodeExtensions.GetValueOrDefault(source, "required", "false"), variables), out required);

            string repeatStr = MathParsing.ReplaceMathVariables("FlightSchool", ConfigNodeExtensions.GetValueOrDefault(source, "required", "false"), variables).Trim();
            switch (repeatStr)
            {
                case "NEVER": repeatable = RepeatMode.NEVER; break;
                case "EXPIRED": repeatable = RepeatMode.EXPIRED; break;
                case "ALWAYS": repeatable = RepeatMode.ALWAYS; break;
                default: repeatable = RepeatMode.NEVER; break;
            }

            tmpList = MathParsing.ReplaceMathVariables("FlightSchool", ConfigNodeExtensions.GetValueOrDefault(source, "classes"), variables).Split(',').ToList();
            tmpList.ForEach((s) => s.Trim());
            //classes = tmpList.ToArray();
            if (!tmpList.TrueForAll(s => s == ""))
                classes = tmpList.ToArray();
            else
                classes = new string[] { };

            minLevel = (int)(MathParsing.ParseMath("FlightSchool", ConfigNodeExtensions.GetValueOrDefault(source, "minLevel", "0"), variables));
            maxLevel = (int)(MathParsing.ParseMath("FlightSchool", ConfigNodeExtensions.GetValueOrDefault(source, "maxLevel", "5"), variables));

            seatMax = (int)(MathParsing.ParseMath("FlightSchool", ConfigNodeExtensions.GetValueOrDefault(source, "seatMax", "-1"), variables));
            seatMin = (int)(MathParsing.ParseMath("FlightSchool", ConfigNodeExtensions.GetValueOrDefault(source, "seatMin", "0"), variables));

            costBase = MathParsing.ParseMath("FlightSchool", ConfigNodeExtensions.GetValueOrDefault(source, "costBase", "0"), variables);
            costSeat = MathParsing.ParseMath("FlightSchool", ConfigNodeExtensions.GetValueOrDefault(source, "costSeat", "0"), variables);

            costTeacher = MathParsing.ParseMath("FlightSchool", ConfigNodeExtensions.GetValueOrDefault(source, "costTeacher", "0"), variables);
            tmpList = MathParsing.ReplaceMathVariables("FlightSchool", ConfigNodeExtensions.GetValueOrDefault(source, "teachClasses"), variables).Split(',').ToList();
            tmpList.ForEach((s) => s.Trim());
            if (!tmpList.TrueForAll(s => s == ""))
                teachClasses = tmpList.ToArray();
            else
                teachClasses = new string[] { };

            teachMinLevel = (int)(MathParsing.ParseMath("FlightSchool", ConfigNodeExtensions.GetValueOrDefault(source, "teachMinLevel", "0"), variables));

            //get the REWARD nodes and replace any variables in there too
            ConfigNode r = source.GetNode("REWARD");
            if (r != null)
            {
                ReplaceValuesInNode(r, variables);

                RewardLog = r.GetNode("FLIGHTLOG");
                r.TryGetValue("XPAmt", ref rewardXP);
            }

            /*  Expiry = source.GetNodes("EXPIRY");
              foreach (ConfigNode node in Expiry)
                  ConfigNodeExtensions.ReplaceValuesInNode(node, variables);*/

            /*string logStr = "Course created";
            logStr += "\nID: " + id;
            logStr += "\nName: " + name;
            logStr += "\nAvailable: " + Available;
            logStr += "\nprereqs: " + preReqs.Length;
            logStr += "\ntime: " + time;
            logStr += "\nrepeatable: " + repeatable;
            logStr += "\nteachMin: " + teachMinLevel;
            logStr += "\nXP: " + rewardXP;
            logStr += "\nLog: ";
            if (RewardLog != null)
                foreach (ConfigNode.Value v in RewardLog.values)
                    logStr += "\n" + v.value;

            UnityEngine.Debug.Log(logStr);*/
        }

        public static void ReplaceValuesInNode(ConfigNode source, Dictionary<string, string> variables)
        {
            foreach (ConfigNode.Value val in source.values)
            {
                val.value = MathParsing.ReplaceMathVariables("FlightSchool", val.value, variables);
            }

            for (int i = 0; i < source.nodes.Count; i++)
            {
                ReplaceValuesInNode(source.nodes[i], variables); //recurse through all attached nodes
            }
        }
    }
}
