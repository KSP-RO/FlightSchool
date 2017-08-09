using System;
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
        public static FlightSchool Instance;

        public List<CourseTemplate> CourseTemplates = new List<CourseTemplate>();
        public List<CourseTemplate> OfferedCourses = new List<CourseTemplate>();
        public List<ActiveCourse> ActiveCourses = new List<ActiveCourse>();

        double LastUT = 0;

        FSGUI GUI = new FSGUI();

        private void OnGUI()
        {
            GUI.SetGUIPositions(OnWindow);
        }

        private void OnWindow(int windowID)
        {
            GUI.DrawGUIs(windowID);
        }

        protected void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            
            FindAllCourseConfigs(); //find all applicable configs
            GenerateOfferedCourses(); //turn the configs into offered courses

        }

        public void FixedUpdate()
        {
            if (HighLogic.CurrentGame == null)
                return;

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
            foreach (ConfigNode course in GameDatabase.Instance.GetConfigNodes("FS_COURSE"))
            {
                CourseTemplates.Add(new CourseTemplate(course));
            }
            Debug.Log("[FS] Found " + CourseTemplates.Count + " courses.");
            //fire an event to let other mods add their configs
        }

        public void GenerateOfferedCourses() //somehow provide some variable options here?
        {
            //convert the saved configs to course offerings
            foreach (CourseTemplate template in CourseTemplates)
            {
                CourseTemplate duplicate = new CourseTemplate(template.sourceNode, true); //creates a duplicate so the initial template is preserved
                duplicate.PopulateFromSourceNode(new Dictionary<string, string>());
                if (duplicate.Available)
                    OfferedCourses.Add(duplicate);
            }

            Debug.Log("[FS] Offering " + OfferedCourses.Count + " courses.");
            //fire an event to let other mods add available courses (where they can pass variables through then)
        }

    }
}
