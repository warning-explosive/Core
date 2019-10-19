namespace SpaceEngineers.Core.CompositionRoot.Test
 {
     using Xunit.Abstractions;

     namespace SpaceEngineers.Core.Utilities.Test
     {
         public abstract class TestBase
         {
             protected ITestOutputHelper Output { get; }

             protected TestBase(ITestOutputHelper output)
             {
                 Output = output;
             }
         }
     }
 }