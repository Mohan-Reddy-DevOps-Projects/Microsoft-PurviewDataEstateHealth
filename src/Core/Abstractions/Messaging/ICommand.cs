namespace DEH.Application.Messaging;

using Domain.Abstractions;
using MediatR;

public interface ICommand : IRequest<Result>, IBaseCommand
{
    
}

public interface ICommand<TReponse> : IRequest<Result<TReponse>>, IBaseCommand
{
}

public interface IBaseCommand
{
}