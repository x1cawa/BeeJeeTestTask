using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace TaskBeeJee.ApiConnector.JsonData
{
    [DataContract]
    public class TaskData
    {
        [DataMember(Name = "id")]
        public int ID { get; set; }
        [DataMember(Name = "username")]
        public string User { get; set; }
        [DataMember(Name = "email")]
        public string Email { get; set; }
        [DataMember(Name = "text")]
        public string Text { get; set; }
        [DataMember(Name = "status")]
        public int Status { get; set; }

        public Main.TaskStatus? EnumStatus
        {
            get
            {
                Main.TaskStatus? res = null;
                try { res = (Main.TaskStatus) Status; }
                catch { }
                return res;
            }
        }

        public bool IsDone
        {
            get
            {
                Main.TaskStatus? status = EnumStatus;
                return status == Main.TaskStatus.Done || status == Main.TaskStatus.DoneEdited;
            }
        }

        public bool IsEdited
        {
            get
            {
                Main.TaskStatus? status = EnumStatus;
                return status == Main.TaskStatus.DoneEdited || status == Main.TaskStatus.InProgressEdited;
            }
        }
    }
}
