﻿using ReactWithDotNet.ThirdPartyLibraries.FramerMotion;

namespace DotNetDomainBoundarySpecifier.WebUI.MainWindow;

sealed record NotificationMessage
{
    public bool IsSuccess { get; init; }

    public string Text { get; init; }

    public int TimeoutInMilliseconds { get; init; } = 2000;
}

delegate Task PublishNotification(NotificationMessage message);

sealed class NotificationHost : Component<NotificationHost.State>
{
    static Element IconFail => new FlexRowCentered(Size(24), Color("#52c41a"))
    {
        new svg(Aria("hidden", "true"), Data("icon", "check-circle"), ViewBox(64, 64, 896, 896), svg.FocusableFalse, Fill("currentColor"), Size("1em"))
        {
            new path { d = "M512 64c247.4 0 448 200.6 448 448S759.4 960 512 960 64 759.4 64 512 264.6 64 512 64zm127.98 274.82h-.04l-.08.06L512 466.75 384.14 338.88c-.04-.05-.06-.06-.08-.06a.12.12 0 00-.07 0c-.03 0-.05.01-.09.05l-45.02 45.02a.2.2 0 00-.05.09.12.12 0 000 .07v.02a.27.27 0 00.06.06L466.75 512 338.88 639.86c-.05.04-.06.06-.06.08a.12.12 0 000 .07c0 .03.01.05.05.09l45.02 45.02a.2.2 0 00.09.05.12.12 0 00.07 0c.02 0 .04-.01.08-.05L512 557.25l127.86 127.87c.04.04.06.05.08.05a.12.12 0 00.07 0c.03 0 .05-.01.09-.05l45.02-45.02a.2.2 0 00.05-.09.12.12 0 000-.07v-.02a.27.27 0 00-.05-.06L557.25 512l127.87-127.86c.04-.04.05-.06.05-.08a.12.12 0 000-.07c0-.03-.01-.05-.05-.09l-45.02-45.02a.2.2 0 00-.09-.05.12.12 0 00-.07 0z" }
        }
    };

    static Element IconSuccess => new FlexRowCentered(Size(24), Color("#52c41a"))
    {
        new svg(Aria("hidden", "true"), Data("icon", "check-circle"), ViewBox(64, 64, 896, 896), svg.FocusableFalse, Fill("currentColor"), Size("1em"))
        {
            new path { d = "M512 64C264.6 64 64 264.6 64 512s200.6 448 448 448 448-200.6 448-448S759.4 64 512 64zm193.5 301.7l-210.6 292a31.8 31.8 0 01-51.7 0L318.5 484.9c-3.8-5.3 0-12.7 6.5-12.7h46.9c10.2 0 19.9 4.9 25.9 13.3l71.2 98.8 157.2-218c6-8.3 15.6-13.3 25.9-13.3H699c6.5 0 10.3 7.4 6.5 12.7z" }
        }
    };

    public static Element CreateNotificationContent(NotificationMessage message)
    {
        if (message is null)
        {
            return null;
        }

        return new FlexRow(Gap(8), WidthFitContent, MaxWidth(400), BackgroundWhite, Theme.BoxShadowForWindow, BorderRadius(5), Padding(8), Border(1, solid, Theme.BorderColor))
        {
            message.IsSuccess ? IconSuccess : IconFail,
            message.Text
        };
    }

    protected override Task constructor()
    {
        Client.ListenEvent<PublishNotification>(OnNotificationPublished);

        return Task.CompletedTask;
    }

    protected override Element render()
    {
        if (state.Message is null)
        {
            return null;
        }

        return new Container
        {
            CreateNotificationContent(state.Message)
        };
    }

    Task OnNotificationPublished(NotificationMessage message)
    {
        state = state with { Message = message };

        Client.GotoMethod(ResetState, TimeSpan.FromMilliseconds(message.TimeoutInMilliseconds));

        return Task.CompletedTask;
    }

    Task ResetState()
    {
        state = new();

        return Task.CompletedTask;
    }

    class Container : PureComponent
    {
        protected override Element render()
        {
            return new AnimatePresence
            {
                children.Count is 0
                    ? null
                    : new motion.div
                    {
                        initial =
                        {
                            height  = 0,
                            opacity = 0,

                            top   = 0,
                            right = 24
                        },
                        animate =
                        {
                            height   = "auto",
                            opacity  = 1,
                            position = "fixed",
                            top      = 30,
                            right    = 24
                        },
                        exit =
                        {
                            height  = 0,
                            opacity = 0
                        },

                        style = { WidthAuto, PositionFixed },
                        children =
                        {
                            children
                        }
                    }
            };
        }
    }

    internal record State
    {
        public NotificationMessage Message { get; init; }
    }
}

static class Notification
{
    public static void NotifyFail(this ReactComponentBase component, string message)
    {
        var notificationMessage = new NotificationMessage
        {
            Text = message
        };

        component.Client.DispatchEvent<PublishNotification>([notificationMessage]);
    }

    public static void NotifySuccess(this ReactComponentBase component, string message)
    {
        var notificationMessage = new NotificationMessage
        {
            IsSuccess = true,
            Text      = message
        };

        component.Client.DispatchEvent<PublishNotification>([notificationMessage]);
    }
    
    public static void ShowResult<T>(this Result<T> result, ReactComponentBase component, string successMessage)
    {
        result.Match(_ => component.NotifySuccess(successMessage), e => component.NotifyFail(e.ToString()));
    }
    
    public static void ShowResult(this Unit unit, ReactComponentBase component, string successMessage)
    {
        unit.Match(() => component.NotifySuccess(successMessage), e => component.NotifyFail(e.ToString()));
    }
}