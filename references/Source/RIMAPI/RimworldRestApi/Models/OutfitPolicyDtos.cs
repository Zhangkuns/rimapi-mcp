// AI-FORK: apparel policy assignment (not immediate apparel changes).
namespace RIMAPI.Models {
public class OutfitPolicyRequest {
 public string PawnId {get;set;}
 public int OutfitId {get;set;}
 public bool DryRun {get;set;}
}}
