namespace SpaceEngineers.Core.CompositionRoot.Test
 {
     using Abstractions;
     using Attributes;
     using Enumerations;
     using Extensions.Attributes;

     [Lifestyle(EnLifestyle.Transient)]
     [Order(3)]
     internal class OpenGenericDecorableServiceDecorator1<T> : IOpenGenericDecorableServiceDecorator<T>,
                                                               IDecorator<IOpenGenericDecorableService<T>>
     {
         public IOpenGenericDecorableService<T> Decoratee { get; }

         public OpenGenericDecorableServiceDecorator1(IOpenGenericDecorableService<T> decorateee)
         {
             Decoratee = decorateee;
         }
     }
 }