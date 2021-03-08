using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace TaskBeeJee.ApiConnector.JsonData
{
    [DataContract]
    public class TasksData
    {
        [DataMember(Name = "tasks")]
        public List<TaskData> Tasks { get; set; }
        [DataMember(Name = "total_task_count")]
        public int TotalCount { get; set; }
    }
}
