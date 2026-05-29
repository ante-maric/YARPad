using System.Security.Authentication;
using AutoMapper;
using AutoMapper.Internal;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;

namespace CodingCell.YARPad;

internal class AutoMapperProfile : Profile
{    
    public AutoMapperProfile()
    {
        CreateMap<ClusterModel, ClusterModel>()
            .ForMember(x => x.OptionalSectionSwitches, x => x.Ignore());
        CreateMap<ClusterConfigSectionSwitch, ClusterConfigSectionSwitch>();
       
        CreateMap<SessionAffinityModel, SessionAffinityModel>();
        CreateMap<SessionAffinityCookieModel, SessionAffinityCookieModel>();
        
        CreateMap<HealthCheckModel, HealthCheckModel>();
        CreateMap<PassiveHealthCheckModel, PassiveHealthCheckModel>();
        CreateMap<ActiveHealthCheckModel, ActiveHealthCheckModel>();
        
        CreateMap<HttpClientModel, HttpClientModel>();
        CreateMap<WebProxyModel, WebProxyModel>();
        
        CreateMap<ForwarderRequestModel, ForwarderRequestModel>();
        CreateMap<DestinationModel, DestinationModel>();
        CreateMap<YarpMetadata, YarpMetadata>();

        CreateMap<RouteModel, RouteModel>()
            .ForMember(x => x.OptionalSectionSwitches, x => x.Ignore());
        CreateMap<RouteConfigSectionSwitch, RouteConfigSectionSwitch>();
        CreateMap<RouteMatchModel, RouteMatchModel>();
        CreateMap<RouteHeaderModel, RouteHeaderModel>();
        CreateMap<RouteQueryParameterModel, RouteQueryParameterModel>();
        CreateMap<RouteTransform, RouteTransform>()
            .Include<RequestHeadersCopyTransform, RequestHeadersCopyTransform>()
            .Include<RequestHeaderOriginalHostTransform, RequestHeaderOriginalHostTransform>()
            .Include<RequestHeaderTransform, RequestHeaderTransform>()
            .Include<PathRemovePrefixTransform, PathRemovePrefixTransform>()
            .Include<PathSetTransform, PathSetTransform>()
            .Include<PathPrefixTransform, PathPrefixTransform>()
            .Include<QueryRouteParameterTransform, QueryRouteParameterTransform>()
            .Include<PathPatternTransform, PathPatternTransform>()
            .Include<QueryValueParameterTransform, QueryValueParameterTransform>()
            .Include<QueryRemoveParameterTransform, QueryRemoveParameterTransform>()
            .Include<HttpMethodChangeTransform, HttpMethodChangeTransform>()
            .Include<RequestHeaderRouteValueTransform, RequestHeaderRouteValueTransform>()
            .Include<RequestHeaderRemoveTransform, RequestHeaderRemoveTransform>()
            .Include<RequestHeadersAllowedTransform, RequestHeadersAllowedTransform>()
            .Include<XForwardedTransform, XForwardedTransform>()
            .Include<ForwardedTransform, ForwardedTransform>()
            .Include<ClientCertTransform, ClientCertTransform>()
            .Include<ResponseHeadersCopyTransform, ResponseHeadersCopyTransform>()
            .Include<ResponseHeaderTransform, ResponseHeaderTransform>()
            .Include<ResponseHeaderRemoveTransform, ResponseHeaderRemoveTransform>()
            .Include<ResponseHeadersAllowedTransform, ResponseHeadersAllowedTransform>()
            .Include<ResponseTrailersCopyTransform, ResponseTrailersCopyTransform>()
            .Include<ResponseTrailerTransform, ResponseTrailerTransform>()
            .Include<ResponseTrailerRemoveTransform, ResponseTrailerRemoveTransform>()
            .Include<ResponseTrailersAllowedTransform, ResponseTrailersAllowedTransform>()
            .Include<CustomTransform, CustomTransform>();

        CreateMap<RequestHeadersCopyTransform, RequestHeadersCopyTransform>();
        CreateMap<RequestHeaderOriginalHostTransform, RequestHeaderOriginalHostTransform>();
        CreateMap<RequestHeaderTransform, RequestHeaderTransform>();
        CreateMap<PathRemovePrefixTransform, PathRemovePrefixTransform>();
        CreateMap<PathSetTransform, PathSetTransform>();
        CreateMap<PathPrefixTransform, PathPrefixTransform>();
        CreateMap<QueryRouteParameterTransform, QueryRouteParameterTransform>();
        CreateMap<PathPatternTransform, PathPatternTransform>();
        CreateMap<QueryValueParameterTransform, QueryValueParameterTransform>();
        CreateMap<QueryRemoveParameterTransform, QueryRemoveParameterTransform>();
        CreateMap<HttpMethodChangeTransform, HttpMethodChangeTransform>();
        CreateMap<RequestHeaderRouteValueTransform, RequestHeaderRouteValueTransform>();
        CreateMap<RequestHeaderRemoveTransform, RequestHeaderRemoveTransform>();
        CreateMap<RequestHeadersAllowedTransform, RequestHeadersAllowedTransform>();
        CreateMap<XForwardedTransform, XForwardedTransform>();
        CreateMap<ForwardedTransform, ForwardedTransform>();
        CreateMap<ClientCertTransform, ClientCertTransform>();
        CreateMap<ResponseHeadersCopyTransform, ResponseHeadersCopyTransform>();
        CreateMap<ResponseHeaderTransform, ResponseHeaderTransform>();
        CreateMap<ResponseHeaderRemoveTransform, ResponseHeaderRemoveTransform>();
        CreateMap<ResponseHeadersAllowedTransform, ResponseHeadersAllowedTransform>();
        CreateMap<ResponseTrailersCopyTransform, ResponseTrailersCopyTransform>();
        CreateMap<ResponseTrailerTransform, ResponseTrailerTransform>();
        CreateMap<ResponseTrailerRemoveTransform, ResponseTrailerRemoveTransform>();
        CreateMap<ResponseTrailersAllowedTransform, ResponseTrailersAllowedTransform>();
        CreateMap<CustomTransform, CustomTransform>();
        CreateMap<CustomTransformParameter, CustomTransformParameter>();
        CreateMap<CustomTransformParameterDefinition, CustomTransformParameterDefinition>();

        CreateMap<PolicyInfo, PolicyInfo>();
        CreateMap<CustomTransformDefinition, CustomTransformDefinition>();
        CreateMap<YARPadConfiguration, YARPadConfiguration>();

        CreateMap<ClusterModel, ClusterConfig>()
            .ForMember(x => x.ClusterId, x => x.MapFrom(y => y.ClusterID))
            .ForMember(x => x.Destinations, x => x.MapFrom(y => y.Destinations.Where(d => d.IsEnabled).ToDictionary(d => d.ID)))
            .ForMember(x => x.Metadata, opt => opt.Condition(y => IsClusterConfigSectionEnabled(y, ClusterConfigSection.Metadata) && y.Metadata.Any()))
            .ForMember(x => x.SessionAffinity, x => x.Condition(y => IsClusterConfigSectionEnabled(y, ClusterConfigSection.SessionAffinity)))
            .ForMember(x => x.HealthCheck, x => x.Condition(y => IsClusterConfigSectionEnabled(y, ClusterConfigSection.HealthCheck)))
            .ForMember(x => x.HttpClient, x => x.Condition(y => IsClusterConfigSectionEnabled(y, ClusterConfigSection.HttpClient)))
            .ForMember(x => x.HttpRequest, x => x.Condition(y => IsClusterConfigSectionEnabled(y, ClusterConfigSection.HttpRequest)));        

        CreateMap<SessionAffinityModel, SessionAffinityConfig>()
            .ForMember(x => x.Enabled, x => x.MapFrom(y => true));
        CreateMap<SessionAffinityCookieModel, SessionAffinityCookieConfig>();

        CreateMap<HealthCheckModel, HealthCheckConfig>();
        CreateMap<PassiveHealthCheckModel, PassiveHealthCheckConfig>();
        CreateMap<ActiveHealthCheckModel, ActiveHealthCheckConfig>();

        CreateMap<IEnumerable<SslProtocols>, SslProtocols>().ConvertUsing(protocols => protocols.ToSingleFlag());
        CreateMap<HttpClientModel, HttpClientConfig>();
        CreateMap<WebProxyModel, WebProxyConfig>();

        CreateMap<ForwarderRequestModel, ForwarderRequestConfig>();
        CreateMap<DestinationModel, DestinationConfig>();

        CreateMap<YarpMetadata, KeyValuePair<string, string>>();

        CreateMap<RouteMatchModel, RouteMatch>();
        CreateMap<RouteHeaderModel, RouteHeader>();
        CreateMap<RouteQueryParameterModel, RouteQueryParameter>();
        CreateMap<RouteModel, RouteConfig>()
            .ForMember(x => x.RouteId, x => x.MapFrom(y => y.RouteID))
            .ForMember(x => x.ClusterId, x => x.MapFrom(y => y.ClusterID))
            .ForMember(x => x.Metadata, x => x.Condition(y => IsRouteConfigSectionEnabled(y, RouteConfigSection.Metadata) && y.Metadata.Any()))
            .ForMember(x => x.Transforms, opt =>
            {
                opt.Condition(y => IsRouteConfigSectionEnabled(y, RouteConfigSection.Transform) && y.Transforms.Any());
                opt.MapFrom(y => y.Transforms.Select(x => x.ToDictionary()).ToList());
            })
            .ForMember(x => x.Order, x => x.Ignore());

        CreateMap<YARPadConfiguration, YarpConfig>()
            .ForMember(x => x.Routes, x => x.MapFrom((src, dest, destMember, context) =>
            {
                var enabledRoutes = src.Routes.Where(r => r.IsEnabled).ToList();
                var routeConfigs = new List<RouteConfig>();
                for (int i = 0; i < enabledRoutes.Count; i++)
                {
                    var routeConfig = context.Mapper.Map<RouteConfig>(enabledRoutes[i]);
                    routeConfig = routeConfig with { Order = i };
                    routeConfigs.Add(routeConfig);
                }
                return routeConfigs;
            }))
            .ForMember(x => x.ChangeToken, x => x.Ignore());

        this.Internal().ForAllMaps((typeMap, map) =>
        {
            // — matching System.Text.Json and Newtonsoft.Json
            map.MaxDepth(64);
        });
    }

    private static bool IsClusterConfigSectionEnabled(ClusterModel clusterModel, ClusterConfigSection section)
    {
        return clusterModel.SectionSwitches.TryGetValue(section, out var sectionSwitch) && sectionSwitch.IsEnabled;
    }

    private static bool IsRouteConfigSectionEnabled(RouteModel routeModel, RouteConfigSection section)
    {
        return routeModel.SectionSwitches.TryGetValue(section, out var sectionSwitch) && sectionSwitch.IsEnabled;
    }
}
