// -----------------------------------------------------------------------
// <copyright file="LoaderMessages.cs" company="ExMod Team">
// Copyright (c) ExMod Team. All rights reserved.
// Licensed under the CC BY-SA 3.0 license.
// </copyright>
// -----------------------------------------------------------------------

namespace Exiled.Loader.Features
{
    using System;

    /// <summary>
    /// A class that contains the different EXILED loader messages.
    /// </summary>
    public static class LoaderMessages
    {
        /// <summary>
        /// Gets the default loader message.
        /// </summary>
        public static string Default => @"
   ▄████████ ▀████    ▐████▀  ▄█   ▄█          ▄████████ ████████▄
  ███    ███   ███▌   ████▀  ███  ███         ███    ███ ███   ▀███
  ███    █▀     ███  ▐███    ███▌ ███         ███    █▀  ███    ███
 ▄███▄▄▄        ▀███▄███▀    ███▌ ███        ▄███▄▄▄     ███    ███
▀▀███▀▀▀        ████▀██▄     ███▌ ███       ▀▀███▀▀▀     ███    ███
  ███    █▄    ▐███  ▀███    ███  ███         ███    █▄  ███    ███
  ███    ███  ▄███     ███▄  ███  ███▌    ▄   ███    ███ ███   ▄███
  ██████████ ████       ███▄ █▀   █████▄▄██   ██████████ ████████▀
                                  ▀                                 ";

        /// <summary>
        /// Gets the Easter egg loader message.
        /// </summary>
        public static string EasterEgg => @"
   ▄████████    ▄████████ ▀████    ▐████▀  ▄█   ▄█          ▄████████ ████████▄
  ███    ███   ███    ███   ███▌   ████▀  ███  ███         ███    ███ ███   ▀███
  ███    █▀    ███    █▀     ███  ▐███    ███▌ ███         ███    █▀  ███    ███
  ███         ▄███▄▄▄        ▀███▄███▀    ███▌ ███        ▄███▄▄▄     ███    ███
▀███████████ ▀▀███▀▀▀        ████▀██▄     ███▌ ███       ▀▀███▀▀▀     ███    ███
         ███   ███    █▄    ▐███  ▀███    ███  ███         ███    █▄  ███    ███
   ▄█    ███   ███    ███  ▄███     ███▄  ███  ███▌    ▄   ███    ███ ███   ▄███
 ▄████████▀    ██████████ ████       ███▄ █▀   █████▄▄██   ██████████ ████████▀
                                                                                ";

        /// <summary>
        /// Gets the Christmas loader message.
        /// </summary>
        public static string Christmas => @"
       __
    .-'  |
   /   <\|        ▄████████ ▀████    ▐████▀  ▄█   ▄█          ▄████████ ████████▄
  /     \'       ███    ███   ███▌   ████▀  ███  ███         ███    ███ ███   ▀███
  |_.- o-o       ███    █▀     ███  ▐███    ███▌ ███         ███    █▀  ███    ███
  / C  -._)\    ▄███▄▄▄        ▀███▄███▀    ███▌ ███        ▄███▄▄▄     ███    ███
 /',        |  ▀▀███▀▀▀        ████▀██▄     ███▌ ███       ▀▀███▀▀▀     ███    ███
|   `-,_,__,'    ███    █▄    ▐███  ▀███    ███  ███         ███    █▄  ███    ███
(,,)====[_]=|    ███    ███  ▄███     ███▄  ███  ███▌    ▄   ███    ███ ███   ▄███
  '.   ____/     ██████████ ████       ███▄ █▀   █████▄▄██   ██████████ ████████▀
   | -|-|_
   |____)_)";

        /// <summary>
        /// Gets the Halloween loader message.
        /// </summary>
        public static string Halloween => @"
@@@@@@@@  @@@  @@@  @@@  @@@       @@@@@@@@  @@@@@@@
@@@@@@@@  @@@  @@@  @@@  @@@       @@@@@@@@  @@@@@@@@
@@!       @@!  !@@  @@!  @@!       @@!       @@!  @@@
!@!       !@!  @!!  !@!  !@!       !@!       !@!  @!@
@!!!:!     !@@!@!   !!@  @!!       @!!!:!    @!@  !@!
!!!!!:      @!!!    !!!  !!!       !!!!!:    !@!  !!!
!!:        !: :!!   !!:  !!:       !!:       !!:  !!!
:!:       :!:  !:!  :!:   :!:      :!:       :!:  !:!
 :: ::::   ::  :::   ::   :: ::::   :: ::::   :::: ::
: :: ::    :   ::   :    : :: : :  : :: ::   :: :  :
                                                       ";

        /// <summary>
        /// Gets the loader message according to the actual month.
        /// </summary>
        /// <returns>The correspondent loader message.</returns>
        public static string GetMessage()
        {
            if (Environment.GetCommandLineArgs().Contains("--defaultloadmessage"))
                return Default;

            if (!Environment.GetCommandLineArgs().Contains("--noeasteregg") && (Loader.Version.ToString().Contains("6.9") || Loader.Random.NextDouble() <= 0.069))
                return EasterEgg;

            return DateTime.Today.Month switch
            {
                12 => Christmas,
                10 => Halloween,
                _ => Default,
            };
        }
    }
}