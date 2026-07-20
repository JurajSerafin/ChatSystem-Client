using Avalonia.Data.Converters;
using Avalonia.Layout;
using System;
using System.Globalization;
using ChatSystem.Client.Core.Domain.User;

namespace ChatSystem.Client.Infrastructure.Presentation.Converters {
    public class MessageBubbleAlignmentConverter : IValueConverter {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
            if (value is UserId senderId && parameter is UserId currentUserId) {
                return senderId == currentUserId ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            }

            return HorizontalAlignment.Left;
        }
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
            throw new NotSupportedException();
        }
    }
}
