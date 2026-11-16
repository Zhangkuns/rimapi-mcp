// AI-FORK: read and assign existing apparel policies.
using System.Net;
using System.Threading.Tasks;
using RIMAPI.Core;
using RIMAPI.Models;
using RIMAPI.Services;
namespace RIMAPI.Controllers {
public class OutfitPolicyController {
 private readonly AiMutationCoordinator _mutations;
 private readonly IOutfitPolicyService _service;
 private readonly IOutfitPolicyCrudService _crud;
 public OutfitPolicyController(IOutfitPolicyService service,IOutfitPolicyCrudService crud,AiMutationCoordinator mutations){_service=service;_crud=crud;_mutations=mutations;}

 [Get("/api/v1/outfit/policy")]
 [EndpointMetadata("List native apparel policies")]
 public async Task GetPolicies(HttpListenerContext context){await ResponseBuilder.SendApiResult(context.Response,_crud.GetPolicies());}

 [Get("/api/v1/outfit/policy/check")]
 [EndpointMetadata("Check one apparel def against an outfit filter")]
 public async Task Check(HttpListenerContext context){await ResponseBuilder.SendApiResult(context.Response,_crud.Check(context.Request.QueryString["outfit_id"],context.Request.QueryString["thing_def"],context.Request.QueryString["special_filter"]));}

 [Post("/api/v1/outfit/policy")]
 [EndpointMetadata("Create/copy a native apparel policy")]
 public async Task Create(HttpListenerContext context){var body=await context.Request.ReadBodyAsync<OutfitPolicyCreateRequest>();await _mutations.SendAsync(context,body,body!=null&&body.DryRun,()=>_crud.Create(body),r=>r.Data!=null&&r.Data.ContainsKey("applied"));}

 [Post("/api/v1/outfit/policy/default")]
 [EndpointMetadata("Move an apparel policy to the default slot")]
 public async Task SetDefault(HttpListenerContext context){var body=await context.Request.ReadBodyAsync<OutfitPolicyIdRequest>();await _mutations.SendAsync(context,body,body!=null&&body.DryRun,()=>_crud.SetDefault(body),r=>r.Data!=null&&r.Data.ContainsKey("applied"));}

 [Post("/api/v1/outfit/policy/delete")]
 [EndpointMetadata("Delete a native apparel policy if unused")]
 public async Task Delete(HttpListenerContext context){var body=await context.Request.ReadBodyAsync<OutfitPolicyIdRequest>();await _mutations.SendAsync(context,body,body!=null&&body.DryRun,()=>_crud.Delete(body),r=>r.Data!=null&&r.Data.ContainsKey("applied"));}

 [Post("/api/v1/outfit/policy/filter")]
 [EndpointMetadata("Allow/disallow one apparel in a native outfit")]
 public async Task SetFilter(HttpListenerContext context){var body=await context.Request.ReadBodyAsync<OutfitPolicyFilterRequest>();await _mutations.SendAsync(context,body,body!=null&&body.DryRun,()=>_crud.SetFilter(body),r=>r.Data!=null&&r.Data.ContainsKey("applied"));}
 [Get("/api/v1/pawn/outfit")]
 [EndpointMetadata("Read the pawn's current apparel policy")]
 public async Task Get(HttpListenerContext context) {
 await ResponseBuilder.SendApiResult(context.Response,_service.Get(context.Request.QueryString["pawn_id"]));
 }
 [Post("/api/v1/pawn/outfit")]
 [EndpointMetadata("Assign an existing apparel policy; game AI handles subsequent clothing changes")]
 public async Task Assign(HttpListenerContext context) {
 var body=await context.Request.ReadBodyAsync<OutfitPolicyRequest>();
 await _mutations.SendAsync(context,body,body!=null && body.DryRun,()=>_service.Assign(body),
 result=>result.Data!=null && (bool)result.Data["changed"]);
 }
}}
