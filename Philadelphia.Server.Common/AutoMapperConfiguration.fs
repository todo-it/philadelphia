namespace Philadelphia.Server.Common

open AutoMapper

type AutoMapperConfiguration =
   abstract member Add: IMapperConfigurationExpression -> unit
