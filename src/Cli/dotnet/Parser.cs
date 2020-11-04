﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Reflection;
using Microsoft.DotNet.Tools.Help;
using Microsoft.DotNet.Tools.MSBuild;
using Microsoft.DotNet.Tools.New;
using Microsoft.DotNet.Tools.NuGet;

namespace Microsoft.DotNet.Cli
{
    public static class Parser
    {
        public static readonly RootCommand RootCommand = new RootCommand();

        // Subcommands
        private static readonly Command[] Subcommands = new Command[]
        {
            AddCommandParser.GetCommand(),
            BuildCommandParser.GetCommand(),
            BuildServerCommandParser.GetCommand(),
            CleanCommandParser.GetCommand(),
            CompleteCommandParser.GetCommand(),
            FsiCommandParser.GetCommand(),
            ListCommandParser.GetCommand(),
            MSBuildCommandParser.GetCommand(),
            NewCommandParser.GetCommand(),
            NuGetCommandParser.GetCommand(),
            PackCommandParser.GetCommand(),
            ParseCommandParser.GetCommand(),
            PublishCommandParser.GetCommand(),
            RemoveCommandParser.GetCommand(),
            RestoreCommandParser.GetCommand(),
            RunCommandParser.GetCommand(),
            SlnCommandParser.GetCommand(),
            StoreCommandParser.GetCommand(),
            TestCommandParser.GetCommand(),
            ToolCommandParser.GetCommand(),
            VSTestCommandParser.GetCommand(),
            HelpCommandParser.GetCommand()
        };

        // Internal commands
        public static readonly Command InstallSuccessCommand = InternalReportinstallsuccessCommandParser.GetCommand();

        // Global options
        public static readonly Option DiagOption = new Option<bool>(new[] { "-d", "--diagnostics" }) { IsHidden = true };

        // Informational options
        public static readonly Option VersionOption = new Option<bool>("--version");

        public static readonly Option InfoOption = new Option<bool>("--info");

        // Argument
        public static readonly Argument DotnetSubCommand = new Argument<string>() { Arity = ArgumentArity.ExactlyOne, IsHidden = true };

        private static Command ConfigureCommandLine(Command rootCommand)
        {
            // Add subcommands
            foreach (var subcommand in Subcommands)
            {
                rootCommand.AddCommand(subcommand);
            }

            //Add internal commands
           rootCommand.AddCommand(InstallSuccessCommand);

            // Add options
            rootCommand.AddGlobalOption(DiagOption);
            rootCommand.AddOption(VersionOption);
            rootCommand.AddOption(InfoOption);

            // Add argument
            rootCommand.AddArgument(DotnetSubCommand);

            return rootCommand;
        }

        public static System.CommandLine.Parsing.Parser Instance { get; } = new CommandLineBuilder(ConfigureCommandLine(RootCommand))
            .UseExceptionHandler(ExceptionHandler)
            .UseHelp()
            .UseHelpBuilder(context => new DotnetHelpBuilder(context.Console))
            .UseValidationMessages(new CommandLineValidationMessages())
            .Build();

        private static void ExceptionHandler(Exception exception, InvocationContext context)
        {
            if (exception is TargetInvocationException)
            {
                exception = exception.InnerException;
            }

            if (exception is Utils.GracefulException)
            {
                context.Console.Error.WriteLine(exception.Message);
            }
            else if (exception is CommandParsingException)
            {
                context.Console.Error.WriteLine(exception.Message);
            }
            else
            {
                context.Console.Error.Write("Unhandled exception: ");
                context.Console.Error.WriteLine(exception.ToString());
            }
            context.ParseResult.ShowHelp();
            context.ResultCode = 1;
        }

        public class DotnetHelpBuilder : HelpBuilder
        {
            public DotnetHelpBuilder(IConsole console) : base(console) { }

            public override void Write(ICommand command)
            {
                var helpArgs = new string[] { "--help" };
                if (command.Equals(RootCommand))
                {
                    Console.Out.WriteLine(HelpUsageText.UsageText);
                }
                else if (command.Name.Equals(NuGetCommandParser.GetCommand().Name))
                {
                    NuGetCommand.Run(helpArgs);
                }
                else if (command.Name.Equals(MSBuildCommandParser.GetCommand().Name))
                {
                    new MSBuildForwardingApp(helpArgs).Execute();
                }
                else if (command.Name.Equals(NewCommandParser.GetCommand().Name))
                {
                    NewCommandShim.Run(helpArgs);
                }
                else if (command.Name.Equals(VSTestCommandParser.GetCommand().Name))
                {
                    new VSTestForwardingApp(helpArgs).Execute();
                }
                else
                {
                    base.Write(command);
                }
            }
        }
    }
}
