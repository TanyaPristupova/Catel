﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Focus.cs" company="Catel development team">
//   Copyright (c) 2011 - 2012 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

#if !WIN80 && !XAMARIN

namespace Catel.Windows.Interactivity
{
#if NETFX_CORE
    using global::Windows.UI.Xaml;
#else
    using System.Windows;
    using System.Windows.Interactivity;
#endif

    using System;
    using System.ComponentModel;
    using Logging;

    /// <summary>
    /// Available moments on which the focus can be set.
    /// </summary>
    public enum FocusMoment
    {
        /// <summary>
        /// Focus when the control is loaded.
        /// </summary>
        Loaded,

        /// <summary>
        /// Focus when a property has changed.
        /// </summary>
        PropertyChanged,

        /// <summary>
        /// Focus when a specific event occurs.
        /// </summary>
        Event
    }

    /// <summary>
    /// Behavior to set focus to a <see cref="FrameworkElement"/>. This behavior sets the focus
    /// only once on the first time the <see cref="Behavior{T}.AssociatedObject"/> is loaded.
    /// </summary>
    /// <remarks>In Silverlight, focusing a control seems very, very hard. Just calling Focus() isn't enough, so a timer is used to set the timer 500 milliseconds after the
    /// user control has been loaded. This is customizable via the <see cref="FocusBehaviorBase.FocusDelay"/> property.</remarks>
    public class Focus : FocusBehaviorBase
    {
        #region Fields
        /// <summary>
        /// The log.
        /// </summary>
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

#if !WINDOWS_PHONE && !NETFX_CORE
        private DynamicEventListener _dynamicEventListener;
#endif
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Focus"/> class.
        /// </summary>
        public Focus()
        {
        }

        #region Properties
        /// <summary>
        /// Gets or sets the focus moment.
        /// <para />
        /// When this value is <see cref="Interactivity.FocusMoment.Loaded" />, no other properties need to be set.
        /// <para />
        /// When this value is <see cref="Interactivity.FocusMoment.PropertyChanged" />, both the <see cref="Source" /> and 
        /// <see cref="PropertyName" /> must be set.
        /// <para />
        /// When this value is <see cref="Interactivity.FocusMoment.Event" />, both the <see cref="Source" /> and 
        /// <see cref="EventName" /> must be set.
        /// </summary>
        /// <value>The focus moment.</value>
        public FocusMoment FocusMoment
        {
            get { return (FocusMoment)GetValue(FocusMomentProperty); }
            set { SetValue(FocusMomentProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for FocusMoment.  This enables animation, styling, binding, etc... 
        /// </summary>
        public static readonly DependencyProperty FocusMomentProperty =
            DependencyProperty.Register("FocusMoment", typeof(FocusMoment), typeof(Focus), new PropertyMetadata(FocusMoment.Loaded));

        /// <summary>
        /// Gets or sets the source. This value is required when the <see cref="FocusMoment" /> property is either 
        /// <see cref="Interactivity.FocusMoment.PropertyChanged" /> or <see cref="Interactivity.FocusMoment.Event" />.
        /// </summary>
        /// <value>The source.</value>
        public object Source
        {
            get { return (object)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc... 
        /// </summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(object), typeof(Focus), 
            new PropertyMetadata(null, (sender, e) => ((Focus)sender).OnSourceChanged(e)));

        /// <summary>
        /// Gets or sets the name of the property. This value is required when the <see cref="FocusMoment" /> property is 
        /// <see cref="Interactivity.FocusMoment.PropertyChanged" />.
        /// </summary>
        /// <value>The name of the property.</value>
        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            set { SetValue(PropertyNameProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for PropertyName.  This enables animation, styling, binding, etc... 
        /// </summary>
        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.Register("PropertyName", typeof(string), typeof(Focus), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the name of the event. This value is required when the <see cref="FocusMoment" /> property is 
        /// <see cref="Interactivity.FocusMoment.Event" />.
        /// </summary>
        /// <value>The name of the event.</value>
        public string EventName
        {
            get { return (string)GetValue(EventNameProperty); }
            set { SetValue(EventNameProperty, value); }
        }

        /// <summary>
        /// Using a DependencyProperty as the backing store for EventName.  This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty EventNameProperty =
            DependencyProperty.Register("EventName", typeof(string), typeof(Focus), new PropertyMetadata(null));
        #endregion

        #region Methods
        /// <summary>
        /// Called when the <see cref="Behavior{T}.AssociatedObject"/> is loaded.
        /// </summary>
        protected override void OnAssociatedObjectLoaded()
        {
            if (!IsFocusAlreadySet && (FocusMoment == FocusMoment.Loaded))
            {
                StartFocus();
            }
        }

        /// <summary>
        /// Called when the event on the <see cref="Source" /> has occurred.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnSourceEventOccurred(object sender, EventArgs e)
        {
            StartFocus();
        }

        /// <summary>
        /// Called when a property on the <see cref="Source" /> has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnSourcePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == PropertyName)
            {
                StartFocus();
            }
        }

        /// <summary>
        /// Called when the source has changed.
        /// </summary>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private void OnSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                switch (FocusMoment)
                {
                    case FocusMoment.Event:
                        
#if WINDOWS_PHONE || NETFX_CORE
                        throw new NotSupportedInPlatformException("Dynamic events are not supported");
#else
                        _dynamicEventListener.EventOccurred -= OnSourceEventOccurred;
                        _dynamicEventListener.UnsubscribeFromEvent();
                        break;
#endif

                    case FocusMoment.PropertyChanged:
                        var sourceAsPropertyChanged = e.OldValue as INotifyPropertyChanged;
                        if (sourceAsPropertyChanged != null)
                        {
                            sourceAsPropertyChanged.PropertyChanged -= OnSourcePropertyChanged;
                        }
                        else
                        {
                            Log.Warning("Cannot unsubscribe from previous source because it does not implement 'INotifyPropertyChanged', this should not be possible and can lead to memory leaks");
                        }
                        break;
                }
            }

            if (e.NewValue != null)
            {
                switch (FocusMoment)
                {
                    case FocusMoment.Event:
#if WINDOWS_PHONE || NETFX_CORE
                        throw new NotSupportedInPlatformException("Dynamic events are not supported");
#else
                        if (string.IsNullOrEmpty(EventName))
                        {
                            throw new InvalidOperationException("Property 'EventName' is required when FocusMode is 'FocusMode.Event'");
                        }

                        _dynamicEventListener = new DynamicEventListener(Source, EventName);
                        _dynamicEventListener.EventOccurred += OnSourceEventOccurred;
                        break;
#endif

                    case FocusMoment.PropertyChanged:
                        if (string.IsNullOrEmpty(PropertyName))
                        {
                            throw new InvalidOperationException("Property 'PropertyName' is required when FocusMode is 'FocusMode.PropertyChanged'");
                        }

                        var sourceAsPropertyChanged = e.NewValue as INotifyPropertyChanged;
                        if (sourceAsPropertyChanged == null)
                        {
                            throw new InvalidOperationException("Source does not implement interface 'INotifyfPropertyChanged', either implement it or change the 'FocusMode'");
                        }

                        sourceAsPropertyChanged.PropertyChanged += OnSourcePropertyChanged;
                        break;
                }
            }
        }
        #endregion
    }
}

#endif