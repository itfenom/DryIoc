﻿using System.ComponentModel.Composition;

namespace DryIoc.MefAttributedModel.UnitTests.CUT
{
    public interface IHandler
    {
    }

    [ExportAll(ContractName = "fast"), ExportWithMetadata(2)]
    public class FastHandler : IHandler
    {
    }

    [ExportAll(ContractName = "slow"), ExportWithMetadata(1)]
    public class SlowHandler : IHandler
    {
    }

    [ExportAll(ContractName = "transact"), ExportWithMetadata(1)]
    public class TransactHandler : IHandler
    {
    }

    [ExportAll, ExportAsDecorator(ContractName = "slow")]
    public class LoggingHandlerDecorator : IHandler
    {
        public IHandler Handler { get; set; }

        public LoggingHandlerDecorator(IHandler handler)
        {
            Handler = handler;
        }
    }

    [ExportAll, ExportAsDecorator(ShouldCompareMetadata = true), ExportWithMetadata(2)]
    public class RetryHandlerDecorator : IHandler
    {
        public IHandler Handler { get; set; }

        public RetryHandlerDecorator(IHandler handler)
        {
            Handler = handler;
        }
    }

    [ExportAll, ExportAsDecorator(ContractName = "transact", ShouldCompareMetadata = true), ExportWithMetadata(1)]
    public class TransactHandlerDecorator : IHandler
    {
        public IHandler Handler { get; set; }

        public TransactHandlerDecorator(IHandler handler)
        {
            Handler = handler;
        }
    }

    [Export(typeof(IHandler)), ExportAsDecorator(ConditionType = typeof(Condition))]
    public class CustomHandlerDecorator : IHandler
    {
        public IHandler Handler { get; set; }

        public CustomHandlerDecorator(IHandler handler)
        {
            Handler = handler;
        }

        public class Condition : IDecoratorCondition
        {
            public bool Check(Request request)
            {
                return request.ImplementationType == typeof(SlowHandler);
            }
        }
    }

    [ExportAll, ExportAsDecorator]
    public class DecoratorWithFastHandlerImport : IHandler
    {
        public IHandler Handler { get; set; }

        public DecoratorWithFastHandlerImport([Import("fast")]IHandler handler)
        {
            Handler = handler;
        }
    }
}
