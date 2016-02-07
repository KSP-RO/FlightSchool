﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP;
using System.IO;

namespace FlightSchool
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class FlightSchool : MonoBehaviour
    {
        List<CourseTemplate> CourseTemplates = new List<CourseTemplate>();
        List<CourseTemplate> OfferedCourses = new List<CourseTemplate>();
        List<ActiveCourse> ActiveCourses = new List<ActiveCourse>();

        double LastUT = 0;

        public void Start()
        {
            FindAllCourseConfigs(); //find all applicable configs
            GenerateOfferedCourses(); //turn the configs into offered courses

        }

        public void FixedUpdate()
        {
            double UT = Planetarium.GetUniversalTime();
            if (LastUT <= 0)
                LastUT = UT;
            double dT = UT - LastUT;
            for (int i = 0; i < ActiveCourses.Count; i++ )
            {
                ActiveCourse course = ActiveCourses[i];
                if (course.ProgressTime(dT)) //returns true when the course completes
                {
                    ActiveCourses.RemoveAt(i);
                    i--;
                }
            }

            LastUT = UT;
        }

        public void FindAllCourseConfigs()
        {
            CourseTemplates.Clear();
            //find all configs and save them
            string defaultPath = KSPUtil.ApplicationRootPath + "/GameData/FlightSchool/Courses/";
            foreach (string file in Directory.GetFiles(defaultPath, "*.cfg"))
            {
                ConfigNode node = ConfigNode.Load(file);
                foreach (ConfigNode course in node.GetNodes("FS_COURSE"))
                {
                    CourseTemplates.Add(new CourseTemplate(course));
                }
            }

            //fire an event to let other mods add their configs
        }

        public void GenerateOfferedCourses() //somehow provide some variable options here?
        {
            //convert the saved configs to course offerings
            foreach (CourseTemplate template in CourseTemplates)
            {
                CourseTemplate duplicate = new CourseTemplate(template.sourceNode); //creates a duplicate so the initial template is preserved
                duplicate.PopulateFromSourceNode(null);
                if (duplicate.Available)
                    OfferedCourses.Add(duplicate);
            }

            //fire an event to let other mods add available courses (where they can pass variables through then)
        }

    }
}
