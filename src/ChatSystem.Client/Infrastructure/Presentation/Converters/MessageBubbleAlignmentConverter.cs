using Avalonia.Data.Converters;
using Avalonia.Layout;
using ChatSystem.Client.Core.Domain.User;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ChatSystem.Client.Infrastructure.Presentation.Converters;

/// <summary>
/// A multivalue converter that determines the horizontal alignment of a chat message bubble.
/// 
/// Aligns messages sent by the current user to the right, and messages from other participants to the left.
/// </summary>
public class MessageBubbleAlignmentConverter : IMultiValueConverter {

    /// <inheritdoc />
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture) {
        if (values is [UserId senderId, UserId currentUserId]) {
            return senderId.Equals(currentUserId)
                ? HorizontalAlignment.Right
                : HorizontalAlignment.Left;
        }

        return HorizontalAlignment.Left;
    }
}