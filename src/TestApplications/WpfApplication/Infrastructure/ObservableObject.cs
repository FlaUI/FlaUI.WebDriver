﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace WpfApplication.Infrastructure
{
    [SuppressMessage("ReSharper", "ExplicitCallerInfoArgument")]
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly Dictionary<string, object> _backingFieldValues = new Dictionary<string, object>();

        /// <summary>
        /// Gets a property value from the internal backing field
        /// </summary>
        protected T GetProperty<T>([CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }
            object value;
            if (_backingFieldValues.TryGetValue(propertyName, out value))
            {
                return (T)value;
            }
            return default(T);
        }

        /// <summary>
        /// Saves a property value to the internal backing field
        /// </summary>
        protected bool SetProperty<T>(T newValue, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }
            if (IsEqual(GetProperty<T>(propertyName), newValue)) return false;
            _backingFieldValues[propertyName] = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Sets a property value to the backing field
        /// </summary>
        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (IsEqual(field, newValue)) return false;
            field = newValue;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> selectorExpression)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(GetNameFromExpression(selectorExpression)));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool IsEqual<T>(T field, T newValue)
        {
            // Alternative: EqualityComparer<T>.Default.Equals(field, newValue);
            return Equals(field, newValue);
        }

        private string GetNameFromExpression<T>(Expression<Func<T>> selectorExpression)
        {
            var body = (MemberExpression)selectorExpression.Body;
            var propertyName = body.Member.Name;
            return propertyName;
        }
    }
}
