namespace SpaceEngineers.Core.CompositionRoot.Test
 {
     using Abstractions;
     using Attributes;
     using Enumerations;
     using Extensions.Attributes;

     [Lifestyle(EnLifestyle.Transient)]
     [Order(2)]
     internal class ConditionalDecorableServiceDecorator2 : IConditionalDecorableServiceDecorator,
                                                            IConditionalDecorator<IConditionalDecorableService, TestConditionAttribute2>
     {
         public IConditionalDecorableService Decoratee { get; }
 
         public ConditionalDecorableServiceDecorator2(IConditionalDecorableService decoratee)
         {
             Decoratee = decoratee;
         }
     }
 }