### master: [![build-test-publish](https://github.com/warning-explosive/Core/actions/workflows/build-test-publish.yml/badge.svg?branch=master)](https://github.com/warning-explosive/Core/actions/workflows/build-test-publish.yml)[![codecov][master-codecov-badge]][codecov]
### develop: [![build-test-publish](https://github.com/warning-explosive/Core/actions/workflows/build-test-publish.yml/badge.svg?branch=develop)](https://github.com/warning-explosive/Core/actions/workflows/build-test-publish.yml)[![codecov][develop-codecov-badge]][codecov]

[codecov]: https://codecov.io/gh/warning-explosive/Core
[master-codecov-badge]: https://codecov.io/gh/warning-explosive/Core/branch/master/graph/badge.svg?token=ABWNWVENC0
[develop-codecov-badge]: https://codecov.io/gh/warning-explosive/Core/branch/develop/graph/badge.svg?token=ABWNWVENC0

### Framework for the core domain development
This repository suggests solution for the core domain development with all the necessary infrastructure implementations on the top of the .NET Core platform.

### Main features

#### DDD & Core domain
    In terms of DDD, core domain is the heart of your information system where magic happens.
    It is fundamental and critical part to the stakeholders that gives competitive advantage on the market and distinguish exact solution of the problem from the others.
    Taking care of infrastructure code and let focus on things that really matters in exact core domain is a key feature of this framework.

#### Encourages best practices and patterns
    Code first - code is the single source of truth and should be written in the way that allows to understand it clearly without any additional technical documentation;
    Microservices, monolith, modular monolith, single database or per-service database - choose any composition type what you want at any stage of your development combining different approaches;
    Dependency injection - intelligent composition of your source code in run-time based on assembly scanning and declarative API for managing code components and providing isolation even in single process deployment;
    Async out of the box - for scalability and, as a result, better throughput;
    Messaging - async messaging with message handler as unit of work and immutable message as input is the model for service communication for predictability and scalability;
    Event sourcing - domain events are basic blocks of state changes through the system persisted in append only event store;
    CQRS - read\write model separation for solving different problems with more appropriate tools;

#### Based on the latest, most popular and sustainable technologies
    Implementing infrastructure for all available technologies isn't reasonable and feasible so basic implementations based on the next technologies:
    Dependency container - [SimpleInjector](https://github.com/simpleinjector/SimpleInjector)
    Message broker - [RabbitMQ](https://github.com/rabbitmq/rabbitmq-dotnet-client)
    Persistence - [PosgreSql](https://github.com/npgsql/npgsql)