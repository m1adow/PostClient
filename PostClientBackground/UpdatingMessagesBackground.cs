using System.Linq;
using Windows.ApplicationModel.Background;

namespace PostClientBackground
{
    public sealed class UpdatingMessagesBackground : IBackgroundTask
    {
        private const string _taskName = "PostClientBackground.UpdatingMessagesBackground";
        private BackgroundTaskDeferral _deferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();

            _deferral.Complete();
        }

        public static IBackgroundTaskRegistration Register()
        {
            var registration = BackgroundTaskRegistration.AllTasks.Select(x => x.Value).FirstOrDefault(x => x.Name == _taskName);
            if (registration != null)
                registration.Unregister(true);

            var taskBuilder = new BackgroundTaskBuilder();
            taskBuilder.Name = _taskName;
            taskBuilder.TaskEntryPoint = _taskName;
            taskBuilder.SetTrigger(new TimeTrigger(60, false));
            taskBuilder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));

            return taskBuilder.Register();
        }
    }
}
