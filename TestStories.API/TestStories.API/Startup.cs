using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using FluentValidation;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TestStories.API.Concrete;
using TestStories.API.Filters;
using TestStories.API.Health;
using TestStories.API.Infrastructure.Filters;
using TestStories.API.Models.RequestModels;
using TestStories.API.Services;
using TestStories.API.Services.Settings.Interfaces;
using TestStories.API.Services.Validators;
using TestStories.API.Validators;
using TestStories.CloudSearch.CloudSearchBALFramework.BusinessBaseClasses;
using TestStories.CloudSearch.Service.CloudSearchEntity;
using TestStories.CloudSearch.Service.Interface;
using TestStories.Common;
using TestStories.Common.Auth;
using TestStories.Common.Blogs;
using TestStories.Common.Configurations;
using TestStories.Common.Events;
using TestStories.Common.MailKit;
using TestStories.Common.Models.Events;
using TestStories.Common.Services.MailerLite;
using TestStories.DataAccess.Entities;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Serilog;
using Swashbuckle.AspNetCore.Filters;

namespace TestStories.API
{
    public class Startup
    {
        static readonly RegionEndpoint Endpoint = RegionEndpoint.GetBySystemName(EnvironmentVariables.AwsRegion);

        public static IConfiguration Configuration { get; } = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                            .AddJsonFile($"appsettings.{EnvironmentVariables.Env}.json", optional: true)
                            .AddEnvironmentVariables()
                            .Build();


        /// <summary>
        ///     This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            //Temporary
            if (EnvironmentVariables.Env == "prod")
            {
                Environment.SetEnvironmentVariable("MAILCHIMP_API_KEY", "43fa17d6137a2a7f937c7c89a9a90dc1-us17");
            }
            else
            {
                Environment.SetEnvironmentVariable("MAILCHIMP_API_KEY", "1c6e189f3e65e5c4f438b389b2a314e8-us10");
            }
            services.AddControllers();
            services.AddMvcCore()
                .AddApiExplorer()
                .AddFluentValidation()
                .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver())
                .AddNewtonsoftJson(options => { options.SerializerSettings.Converters.Add(new StringEnumConverter()); });

            services.AddLogging(configure =>
            {
                configure.AddSerilog(dispose: true,
                    logger: new LoggerConfiguration()
                        .ReadFrom.Configuration(Configuration)
                        .CreateLogger());
            });

            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.Configure<EmailSettings>(Configuration.GetSection("Email"));
            services.Configure<ImageSettings>(Configuration.GetSection("ImageSettings"));
            services.Configure<MailChimpSettings>(Configuration.GetSection("Mailchimp"));
            services.Configure<MailerLiteSettings>(Configuration.GetSection("MailerLite"));
            services.AddDbContext<TestStoriesContext>(options => options.UseSqlServer(EnvironmentVariables.TestStoriesDbConnection));

             var clientAppUrl = EnvironmentVariables.ClientUiUrl.Replace("http://", string.Empty).Replace("https://", string.Empty);
            var adminAppUrl = EnvironmentVariables.AdminUiUrl.Replace("http://", string.Empty).Replace("https://", string.Empty);
            Log.Debug($"Started {adminAppUrl}");
            Log.Debug($"Env var {EnvironmentVariables.Env}");
            services.AddCors(options =>
            {
                if (EnvironmentVariables.Env == "prod")
                {
                    options.AddPolicy("AllowSpecificOrigins",
                    builder => builder
                              .WithOrigins("http://" + clientAppUrl, "https://" + clientAppUrl, "http://www." + clientAppUrl, "https://www." + clientAppUrl)
                              .WithOrigins("http://" + adminAppUrl, "https://" + adminAppUrl)
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                    );
                }
                else
                {
                    options.AddPolicy("AllowSpecificOrigins",
                   builder => builder
                             .WithOrigins("http://" + "localhost:3000", "https://" + "localhost:3000")
                             .WithOrigins("http://" + "localhost:3001", "https://" + "localhost:3001")
                             .WithOrigins("http://" + clientAppUrl, "https://" + clientAppUrl, "http://www." + clientAppUrl, "https://www." + clientAppUrl)
                             .WithOrigins("http://" + adminAppUrl, "https://" + adminAppUrl)
                             .AllowAnyHeader()
                             .AllowAnyMethod()
                   );
                }
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1",
              new OpenApiInfo
              {
                  Title = EnvironmentVariables.ServiceName,
                  Version = EnvironmentVariables.ServiceVersion,
                  Description = "ASPNETCORE_ENVIRONMENT: " + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") + "<br>" + "ENVIRONMENT: " + Environment.GetEnvironmentVariable("ENVIRONMENT") + "<br>"
              });
                options.EnableAnnotations();
                options.SchemaFilter<NullableTypeSchemaFilter>();
                //options.SchemaFilter<DefaultValueSchemaFilter>();

                var filePath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
                if (File.Exists(filePath))
                {
                    options.IncludeXmlComments(filePath, true);
                }
                options.OperationFilter<FormFileSwaggerFilter>();
                options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

                options.AddSecurityDefinition("Bearer",
                new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                          {
                              Reference = new OpenApiReference
                              {
                                  Type = ReferenceType.SecurityScheme,
                                  Id = "Bearer"
                              }
                          },
                         Array.Empty<string>()
                    }
                });
                options.CustomSchemaIds(x => x.FullName);
            });

            // JWT Setting

            var token = new TokenManagement
            {
                Secret = EnvironmentVariables.JwtSecret,
                Issuer = EnvironmentVariables.JwtIssuer,
                Audience = EnvironmentVariables.JwtAudience,
                AccessExpiration = EnvironmentVariables.JwtAccessExpiration,
                RefreshExpiration = EnvironmentVariables.JwtRefreshExpiration
            };
            var secret = Encoding.ASCII.GetBytes(token.Secret);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(token.Secret)),
                    ValidIssuer = token.Issuer,
                    ValidAudience = token.Audience,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddDefaultAWSOptions(new AWSOptions { Region = RegionEndpoint.GetBySystemName(EnvironmentVariables.AwsRegion) })
                .AddAWSService<AmazonSimpleNotificationServiceClient>()
                .AddAWSService<AmazonS3Client>();

            if (!string.IsNullOrEmpty(EnvironmentVariables.DataProtectionSsmPrefix))
            {
                services
                    .AddAWSService<AmazonSimpleSystemsManagementClient>()
                    .AddDataProtection()
                    .PersistKeysToAWSSystemsManager(EnvironmentVariables.DataProtectionSsmPrefix);
            }
            services.AddSingleton<AmazonSimpleNotificationServiceClient>();
            services.AddScoped(typeof(IPublishEvent<>), typeof(PublishEmail<>));
            services.AddScoped(typeof(IPublishBlog), typeof(PublishBlog));
            services.AddTransient<IMailerLiteService, MailerLiteService>();
            services.AddScoped<CustomAuthorizationFilter>();
            services.AddTransient<IUserWriteService, UserWriteService>();
            services.AddTransient<IUserReadService, UserReadService>();
            services.AddTransient<IPartnerReadService, PartnerReadService>();
            services.AddTransient<IPartnerWriteService, PartnerWriteService>();
            services.AddTransient<IMediaReadService, MediaReadService>();
            services.AddTransient<IMediaWriteService, MediaWriteService>();
            services.AddTransient<ITopicReadService, TopicReadService>();
            services.AddTransient<ITopicWriteService, TopicWriteService>();
            services.AddTransient<ISeriesWriteService, SeriesWriteService>();
            services.AddTransient<ISeriesReadService, SeriesReadService>();
            services.AddTransient<IToolsReadService, ToolsReadService>();
            services.AddTransient<IToolsWriteService, ToolsWriteService>();
            services.AddTransient<ICommonReadService, CommonReadService>();
            services.AddTransient<ICommonWriteService, CommonWriteService>();
            services.AddTransient<IHomePageReadService, HomePageReadService>();
            services.AddScoped<IImageMigration, ImageMigration>();
            services.AddTransient<ISettingReadService, SettingReadService>();
            services.AddTransient<ISettingWriteService, SettingWriteService>();
            services.AddTransient<IAccountRepository, EFAccountRepository>();
            services.AddTransient<IEditorPickReadService, EditorPickReadService>();
            services.AddTransient<IEditorPickWriteService, EditorPickWriteService>();
            services.AddTransient<IExperimentReadService, ExperimentReadService>();
            services.AddTransient<IExperimentWriteService, ExperimentWriteService>();
            services.AddTransient<ISeoRepository, EFSeoRepository>();
            services.AddTransient<IS3BucketService, S3BucketService>();
            services.AddTransient<IVideoPipelineService, VideoPipelineService>();
            services.AddTransient<IToolTypeReadService, ToolTypeReadService>();
            services.AddTransient<IToolTypeWriteService, ToolTypeWriteService>();
            services.AddTransient<IUserMediaReadService, UserMediaReadService>();
            services.AddTransient<IUserMediaWriteService, UserMediaWriteService>();
            services.AddTransient<IUserTypesService, UserTypesService>();
            services.AddTransient<ISeriesStandaloneReadService, SeriesStandaloneReadService>();
            services.AddTransient<IMediaStandaloneReadService, MediaStandaloneReadService>();

            services.AddTransient<IValidator<AddUserModel>, AddUserModelValidator>();
            services.AddTransient<IValidator<AddBannerModel>, AddBannerModelValidator>();
            services.AddTransient<IValidator<AddEmbedMediaModel>, AddEmbedMediaModelValidator>();
            services.AddTransient<IValidator<AddTopicModel>, AddTopicModelValidator>();
            services.AddTransient<IValidator<EditTopicModel>, EditTopicModelValidator>();
            services.AddTransient<IValidator<AddSeriesModel>, AddSeriesModelValidator>();
            services.AddTransient<IValidator<EditSeriesModel>, EditSeriesModelValidator>();
            services.AddTransient<IValidator<SaveFeaturedCarouselSettingsModel>, SaveFeaturedCarouselSettingsModelValidator>();
            services.AddTransient<IValidator<SaveFeaturedCarouselSettingsModel>, SaveFeaturedCarouselSettingsModelValidator>();
            services.AddTransient<IValidator<FilterSeriesStandaloneModel>, FilterSeriesStandaloneModelValidator>();
            services.AddTransient<IValidator<FilterMediaStandaloneModel>, FilterMediaStandaloneModelValidator>();

            // aws cloud dependency injection
            services.AddTransient<ICloudUserSearchProvider, UserCloudSearch>();
            services.AddTransient<ICloudMediaSearchProvider, MediaCloudSearch>();
            services.AddTransient<ICloudTopicToolSeriesProvider, TopicToolSeriesCloudSearch>();

            services.AddHealthChecksUI(setupSettings: settings =>
            {
                settings
                    .AddHealthCheckEndpoint("Liveness Checks", "http://localhost/healthz")
                    .AddHealthCheckEndpoint("Readiness Checks", "http://localhost/readyz")
                    .SetEvaluationTimeInSeconds(30)
                    .SetMinimumSecondsBetweenFailureNotifications(60);
            });
            services.AddHealthChecksUI().AddSqlServerStorage(EnvironmentVariables.TestStoriesDbConnection);
            services.AddSingleton<LivenessHealthCheck>();
            var s3Tags = new[] { "s3", "readyz" };
            services.AddHealthChecks()
                .AddCheck("healthz", () => HealthCheckResult.Healthy(),
                    new[] { "healthz" })
                .AddLivenessHealthCheck("liveness", HealthStatus.Unhealthy, new List<string> { "healthz" })
                .AddS3Check(name: "video-in-bucket(s3)",
                    setup: setup =>
                    {
                        setup.BucketName = EnvironmentVariables.S3BucketVideoIn;
                        setup.Endpoint = Endpoint;
                    },
                    tags: s3Tags,
                    failureStatus: HealthStatus.Unhealthy)
                .AddS3Check(name: "video-transcoded-bucket(s3)",
                    setup: setup =>
                    {
                        setup.BucketName = EnvironmentVariables.S3BucketVideoOut;
                        setup.Endpoint = Endpoint;
                    },
                    tags: s3Tags,
                    failureStatus: HealthStatus.Unhealthy)
                .AddS3Check(name: "media-bucket(s3)",
                    setup: setup =>
                    {
                        setup.BucketName = EnvironmentVariables.S3BucketMedia;
                        setup.Endpoint = Endpoint;
                    },
                    tags: s3Tags,
                    failureStatus: HealthStatus.Unhealthy)
                .AddUrlGroup(new Uri($"{EnvironmentVariables.AuthServiceUrl}/healthz"), "auth-service",
                    tags: new[] { "auth", "readyz" })
                //.AddPrivateMemoryHealthCheck(1000, "memory-check", tags: new[] { "memory", "healthz" })
                .AddSqlServer(
                    EnvironmentVariables.TestStoriesDbConnection,
                    "SELECT 1;",
                    "database-access",
                    HealthStatus.Unhealthy,
                    new[] { "db", "sql", "readyz" })
                .AddApplicationInsightsPublisher();

            Log.Debug($"Started {EnvironmentVariables.ServiceName}");
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// <summary>
        ///     Configuration Method
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            var policyCollection = new HeaderPolicyCollection()
            .AddFrameOptionsDeny()
            .AddXssProtectionBlock()
            .AddContentTypeOptionsNoSniff()
            .AddStrictTransportSecurityMaxAgeIncludeSubDomains(maxAgeInSeconds: 60 * 60 * 24 * 365) // maxage = one year in seconds
            .AddReferrerPolicyStrictOriginWhenCrossOrigin()
            .RemoveServerHeader()
            .AddContentSecurityPolicy(builder =>
            {
                builder.AddObjectSrc().None();
                builder.AddFormAction().Self();
                builder.AddFrameAncestors().None();
            });
            app.UseSecurityHeaders(policyCollection);



            app.UseHealthChecks("/healthz",
                HealthCheckOptions("healthz"));
            app.UseHealthChecks("/readyz",
                HealthCheckOptions("readyz"));

            app.UseHealthChecksUI(config =>
            {
                config.UIPath = "/health-ui";
                config.ApiPath = "/health-api";
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                app.UseExceptionHandler("/home/error");
            }

            app.UseAuthentication();
            app.UseMiddleware<RequestMiddleware>();


            if (!env.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "TestStories V1"); });
            }
            app.UseCors("AllowSpecificOrigins");
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static HealthCheckOptions HealthCheckOptions(string tag)
        {
            return new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(tag),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
                ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                    [HealthStatus.Degraded] = StatusCodes.Status200OK,
                    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                }
            };
        }
    }
}
