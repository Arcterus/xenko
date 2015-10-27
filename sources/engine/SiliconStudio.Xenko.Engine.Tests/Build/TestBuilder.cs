﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
//using Xenko.Framework.Build.Pipeline;
using SiliconStudio.Core.IO;

// Temporary disabling build unit tests, until we move them to a new csproj with correct dependencies

namespace SiliconStudio.Core.Tests.Build
{
    //[TestFixture]
    //public class TestBuilder
    //{
    //    public class SimpleCommand : Command
    //    {
    //        static int counter = 0;
    //        int id;
    //        bool veryLong;

    //        public SimpleCommand(bool verylong = false)
    //        {
    //            veryLong = verylong;
    //        }

    //        public override void DoCommand(CommandResult result)
    //        {
    //            id = ++counter;
    //            Console.WriteLine("Starting SimpleCommand " + id);
    //            int k = 0;
    //            int max = veryLong ? 100000 : 10000;

    //            for (int i = 0; i < max; ++i)
    //            {
    //                for (int j = 0; j < 10000; ++j)
    //                {
    //                    k += (j >> 5) + (((k * i) % 59743) >> 4);
    //                }
    //            }
    //            Console.WriteLine("Finished SimpleCommand " + id);

    //            result.Status = ResultStatus.Successful;
    //        }

    //        public override bool ShouldExecute()
    //        {
    //            return true;
    //        }

    //        public override void Cancel()
    //        {
    //            throw new NotImplementedException();
    //        }

    //        protected override void ComputeParameterHash(Stream stream)
    //        {
    //            stream.Write(BitConverter.GetBytes(id), 0, sizeof(int));
    //        }
    //    }

    //    [Test]
    //    public void TestExecuteDummySequencialCommands()
    //    {
    //        VirtualFileSystem.MountFileSystem("/source", ".");
    //        Builder builder = new Builder("TestExecuteDummySequencialCommands");
    //        Command prevCommand = new SimpleCommand();
    //        builder.RootCommands.Add(prevCommand);

    //        for (int i = 0; i < 20; ++i)
    //        {
    //            var cmd = new SimpleCommand();
    //            prevCommand.AddNextCommand(cmd);
    //            prevCommand = cmd;
    //        }

    //        builder.Run();
    //    }

    //    [Test]
    //    public void TestExecuteDummyParallelizedCommands()
    //    {
    //        VirtualFileSystem.MountFileSystem("/source", ".");
    //        Builder builder = new Builder("TestExecuteDummyParallelizedCommands");
    //        Command rootCommand = new SimpleCommand();
    //        builder.RootCommands.Add(rootCommand);

    //        for (int i = 0; i < 20; ++i)
    //        {
    //            var cmd = new SimpleCommand();
    //            rootCommand.AddNextCommand(cmd);
    //        }

    //        builder.Run();
    //    }
    //    [Test]
    //    public void TestExecuteDummyParallelizedCommandsWithSomeVeryLong()
    //    {
    //        VirtualFileSystem.MountFileSystem("/source", ".");
    //        Builder builder = new Builder("TestExecuteDummyParallelizedCommandsWithSomeVeryLong");
    //        Command rootCommand = new SimpleCommand();
    //        builder.RootCommands.Add(rootCommand);

    //        for (int i = 0; i < 20; ++i)
    //        {
    //            var cmd = new SimpleCommand(i > 0 && i < 5);
    //            rootCommand.AddNextCommand(cmd);
    //        }

    //        builder.Run();
    //    }
    //}
}
