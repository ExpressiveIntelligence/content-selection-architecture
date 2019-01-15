﻿
namespace Controllers
{
    // fixme: stub. Fill in once I've figured out more general patterns for controllers.
    public interface IController
    {
        // Adds the knowledge sources and does any other initialization pre-execution.
        void Initialize();

        // Starts the controller executing. 
        void Execute(); 
    }
}