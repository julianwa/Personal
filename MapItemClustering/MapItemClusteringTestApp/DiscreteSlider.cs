using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MapItemClusteringTestApp
{
    public class DiscreteSlider : Slider
    {
        private bool _Busy;

        #region DiscreteValue (DependencyProperty)

        /// <summary>
        /// The discrete value of the slider.
        /// </summary>
        public int DiscreteValue
        {
            get { return (int)GetValue(DiscreteValueProperty); }
            set { SetValue(DiscreteValueProperty, value); }
        }
        public static readonly DependencyProperty DiscreteValueProperty =
            DependencyProperty.Register("DiscreteValue", typeof(int), typeof(DiscreteSlider),
            new PropertyMetadata(0, new PropertyChangedCallback(OnDiscreteValueChanged)));

        private static void OnDiscreteValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DiscreteSlider)d).OnDiscreteValueChanged(e);
        }

        protected virtual void OnDiscreteValueChanged(DependencyPropertyChangedEventArgs e)
        {
            _Busy = true;

            Value = (int)e.NewValue;
            base.OnValueChanged((int)e.OldValue, (int)e.NewValue);

            if (DiscreteValueChanged != null)
            {
                DiscreteValueChanged(this, EventArgs.Empty);
            }

            _Busy = false;
        }

        #endregion

        public event EventHandler DiscreteValueChanged;

        protected override void OnValueChanged(double oldValue, double newValue)
        {
            if (!_Busy)
            {
                _Busy = true;

                if (SmallChange != 0)
                {
                    int newDiscreteValue = (int)(Math.Round(newValue / SmallChange) * SmallChange);

                    if (newDiscreteValue != DiscreteValue)
                    {
                        Value = newDiscreteValue;
                        base.OnValueChanged(DiscreteValue, newDiscreteValue);
                        DiscreteValue = newDiscreteValue;
                    }
                }
                else
                {
                    base.OnValueChanged(oldValue, newValue);
                }

                _Busy = false;
            }
        }
    }
}
