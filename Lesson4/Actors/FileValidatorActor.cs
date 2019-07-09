﻿using System.IO;
using Akka.Actor;


namespace Lesson4.Actors
{
    public class FileValidatorActor : UntypedActor
    {
        private readonly IActorRef _consoleWriterActor;
        private readonly IActorRef _tailCoordinatorActor;

        public FileValidatorActor(IActorRef consoleWriterActor, IActorRef tailCoordinatorActor)
        {
            _consoleWriterActor = consoleWriterActor;
            _tailCoordinatorActor = tailCoordinatorActor;
        }

        protected override void OnReceive(object message)
        {
            var msg = message as string;
            if (string.IsNullOrEmpty(msg))
            {
                // signal that the user needs to supply an input
                _consoleWriterActor.Tell(new Lesson4.Messages.NullInputErrorMsg("Input was blank. Please try again.\n"));

                // tell sender to continue doing its thing (whatever that may be, this actor doesn't care)
                Sender.Tell(new Lesson4.Messages.ContinueProcessingMsg());
            }
            else
            {
                var valid = IsFileUri(msg);
                if (valid)
                {
                    // signal successful input
                    var successMsg = new Messages.InputSuccessMsg(string.Format("Starting processing for {0}", msg));

                    _consoleWriterActor.Tell(successMsg);

                    // start coordinator
                    _tailCoordinatorActor.Tell(new TailCoordinatorActor.StartTail(msg, _consoleWriterActor));
                }
                else
                {
                    // signal that input was bad
                    _consoleWriterActor.Tell(new Messages.ValidationErrorMsg(string.Format("{0} is not an existing URI on disk.", msg)));

                    // tell sender to continue doing its thing (whatever that may be, this actor doesn't care)
                    Sender.Tell(new Messages.ContinueProcessingMsg());
                }
            }



        }

        /// <summary>
        /// Checks if file exists at path provided by user.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static bool IsFileUri(string path)
        {
            return File.Exists(path);
        }
    }
}
