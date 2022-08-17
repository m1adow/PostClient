using PostClient.ViewModels.Infrastructure;
using System.Numerics;
using System.Windows.Input;
using Windows.UI.Composition;
using Windows.UI.Xaml;

namespace PostClient.ViewModels
{
    public sealed class AnimationsImplementationViewModel : ViewModelBase
    {
        public ICommand PointerEnteredHandlerCommand { get; }

        public ICommand PointerExitedHandlerCommand { get; }

        private readonly Compositor _compositor = Window.Current.Compositor;
        private SpringVector3NaturalMotionAnimation _springAnimation;

        public AnimationsImplementationViewModel()
        {
            PointerEnteredHandlerCommand = new RelayCommand(PointerEnteredHandler);
            PointerExitedHandlerCommand = new RelayCommand(PointerExitedHandler);
        }

        private void CreateOrUpdateSpringAnimation(float finalValue)
        {
            if (_springAnimation == null)
            {
                _springAnimation = _compositor.CreateSpringVector3Animation();
                _springAnimation.Target = "Scale";
            }

            _springAnimation.FinalValue = new Vector3(finalValue);
        }

        private void PointerExitedHandler(object parameter)
        {
            CreateOrUpdateSpringAnimation(1f);

            (parameter as UIElement).StartAnimation(_springAnimation);
        }

        private void PointerEnteredHandler(object parameter)
        {
            CreateOrUpdateSpringAnimation(1.1f);

            (parameter as UIElement).StartAnimation(_springAnimation);
        } 
    }
}
