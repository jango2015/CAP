﻿using System;
using DotNetCore.CAP.Models;

namespace DotNetCore.CAP.Processor.States
{
    public class FailedState : IState
    {
        public const string StateName = "Failed";

        public TimeSpan? ExpiresAfter => TimeSpan.FromDays(15);

        public string Name => StateName;

        public void Apply(CapPublishedMessage message, IStorageTransaction transaction)
        {
        }

        public void Apply(CapReceivedMessage message, IStorageTransaction transaction)
        {
        }
    }
}