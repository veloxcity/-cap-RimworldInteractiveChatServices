using RimWorld;
using Verse;

namespace CAP_ChatInteractive
{
    public class Alert_ViewersInQueue : Alert
    {
        public Alert_ViewersInQueue()
        {
            this.defaultLabel = "Viewers Waiting in Queue";
            this.defaultPriority = AlertPriority.Medium;
        }

        public override TaggedString GetExplanation()
        {
            var queueManager = CAPChatInteractiveMod.GetPawnAssignmentManager();
            int queueSize = queueManager.GetQueueSize();

            if (queueSize == 0)
                return "No viewers are currently waiting in the pawn queue.";

            return $"{queueSize} viewer{(queueSize > 1 ? "s" : "")} {(queueSize > 1 ? "are" : "is")} waiting in the pawn queue for assignment.\n\nClick to open the Pawn Queue management dialog.";
        }

        public override AlertReport GetReport()
        {
            var queueManager = CAPChatInteractiveMod.GetPawnAssignmentManager();
            int queueSize = queueManager.GetQueueSize();

            // Only show alert if there are viewers in queue
            return queueSize > 0;
        }

        protected override void OnClick()
        {
            // Open the PawnQueue dialog when alert is clicked
            Find.WindowStack.Add(new Dialog_PawnQueue());
        }
    }
}