using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartedIn.CrossCutting.DTOs.ResponseDTO.SignNowResponseDTO
{
    public class SignNowDocumentFullResponseDTO
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("parent_id")]
        public string ParentId { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("document_name")]
        public string DocumentName { get; set; }

        [JsonProperty("page_count")]
        public int PageCount { get; set; }

        [JsonProperty("created")]
        public string Created { get; set; }

        [JsonProperty("updated")]
        public string Updated { get; set; }

        [JsonProperty("original_filename")]
        public string OriginalFilename { get; set; }

        [JsonProperty("origin_document_id")]
        public string OriginDocumentId { get; set; }

        [JsonProperty("owner")]
        public string Owner { get; set; }

        [JsonProperty("owner_name")]
        public string OwnerName { get; set; }

        [JsonProperty("template")]
        public bool Template { get; set; }

        [JsonProperty("origin_user_id")]
        public string OriginUserId { get; set; }

        [JsonProperty("version_time")]
        public string VersionTime { get; set; }

        [JsonProperty("thumbnail")]
        public ThumbnailDto Thumbnail { get; set; }

        [JsonProperty("roles")]
        public List<RoleDto> Roles { get; set; }

        [JsonProperty("viewer_roles")]
        public List<RoleDto> ViewerRoles { get; set; }

        [JsonProperty("approver_roles")]
        public List<RoleDto> ApproverRoles { get; set; }

        [JsonProperty("attachments")]
        public List<AttachmentDto> Attachments { get; set; }

        [JsonProperty("checks")]
        public List<CheckDto> Checks { get; set; }

        [JsonProperty("document_group_info")]
        public DocumentGroupInfoDto DocumentGroupInfo { get; set; }

        [JsonProperty("document_group_template_info")]
        public List<DocumentGroupTemplateInfoDto> DocumentGroupTemplateInfo { get; set; }

        [JsonProperty("integrations")]
        public List<IntegrationDto> Integrations { get; set; }

        [JsonProperty("settings")]
        public SettingsDto Settings { get; set; }

        [JsonProperty("signing_session_settings")]
        public SigningSessionSettingsDto SigningSessionSettings { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonProperty("enumeration_options")]
        public List<EnumerationOptionDto> EnumerationOptions { get; set; }

        [JsonProperty("exported_to")]
        public List<string> ExportedTo { get; set; }

        [JsonProperty("fields")]
        public List<FieldDto> Fields { get; set; }

        [JsonProperty("field_invites")]
        public List<FieldInviteDto> FieldInvites { get; set; }

        [JsonProperty("viewer_field_invites")]
        public List<FieldInviteDto> ViewerFieldInvites { get; set; }

        [JsonProperty("approver_field_invites")]
        public List<FieldInviteDto> ApproverFieldInvites { get; set; }

        [JsonProperty("field_validators")]
        public List<FieldValidatorDto> FieldValidators { get; set; }

        [JsonProperty("hyperlinks")]
        public List<HyperlinkDto> Hyperlinks { get; set; }

        [JsonProperty("inserts")]
        public List<InsertDto> Inserts { get; set; }

        [JsonProperty("notary_invites")]
        public List<NotaryInviteDto> NotaryInvites { get; set; }

        [JsonProperty("originator_organization_settings")]
        public List<OrganizationSettingDto> OriginatorOrganizationSettings { get; set; }

        [JsonProperty("payments")]
        public List<PaymentDto> Payments { get; set; }

        [JsonProperty("radio_buttons")]
        public List<RadioButtonDto> RadioButtons { get; set; }

        [JsonProperty("seals")]
        public List<SealDto> Seals { get; set; }

        [JsonProperty("signatures")]
        public List<SignatureDto> Signatures { get; set; }

        [JsonProperty("requests")]
        public List<RequestDto> Requests { get; set; }

        [JsonProperty("routing_details")]
        public List<RoutingDetailDto> RoutingDetails { get; set; }

        [JsonProperty("texts")]
        public List<TextDto> Texts { get; set; }

        [JsonProperty("originator_logo")]
        public string OriginatorLogo { get; set; }

        [JsonProperty("pages")]
        public List<PageDto> Pages { get; set; }

        [JsonProperty("lines")]
        public List<LineDto> Lines { get; set; }

        [JsonProperty("share_info")]
        public ShareInfoDto ShareInfo { get; set; }
    }

    public class ThumbnailDto
    {
        [JsonProperty("small")]
        public string Small { get; set; }

        [JsonProperty("medium")]
        public string Medium { get; set; }

        [JsonProperty("large")]
        public string Large { get; set; }
    }

    public class RoleDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class AttachmentDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }
    }

    public class CheckDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class DocumentGroupInfoDto
    {
        [JsonProperty("document_group_id")]
        public string DocumentGroupId { get; set; }

        [JsonProperty("document_group_name")]
        public string DocumentGroupName { get; set; }

        [JsonProperty("invite_id")]
        public string InviteId { get; set; }

        [JsonProperty("invite_status")]
        public string InviteStatus { get; set; }

        [JsonProperty("sign_as_merged")]
        public bool SignAsMerged { get; set; }

        [JsonProperty("doc_count_in_group")]
        public int DocCountInGroup { get; set; }

        [JsonProperty("freeform_invite")]
        public FreeformInviteDto FreeformInvite { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class FreeformInviteDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class DocumentGroupTemplateInfoDto
    {
        [JsonProperty("template_id")]
        public string TemplateId { get; set; }

        [JsonProperty("template_name")]
        public string TemplateName { get; set; }
    }

    public class IntegrationDto
    {
        [JsonProperty("integration_id")]
        public string IntegrationId { get; set; }

        [JsonProperty("integration_name")]
        public string IntegrationName { get; set; }
    }

    public class SettingsDto
    {
        [JsonProperty("setting_id")]
        public string SettingId { get; set; }

        [JsonProperty("setting_value")]
        public string SettingValue { get; set; }
    }

    public class SigningSessionSettingsDto
    {
        [JsonProperty("welcome_message")]
        public string WelcomeMessage { get; set; }
    }

    public class EnumerationOptionDto
    {
        [JsonProperty("option_id")]
        public string OptionId { get; set; }

        [JsonProperty("option_value")]
        public string OptionValue { get; set; }
    }

    public class FieldDto
    {
        [JsonProperty("field_id")]
        public string FieldId { get; set; }

        [JsonProperty("field_name")]
        public string FieldName { get; set; }
    }

    public class FieldInviteDto
    {
        [JsonProperty("invite_id")]
        public string InviteId { get; set; }

        [JsonProperty("field_id")]
        public string FieldId { get; set; }
    }

    public class FieldValidatorDto
    {
        [JsonProperty("validator_id")]
        public string ValidatorId { get; set; }

        [JsonProperty("validation_rule")]
        public string ValidationRule { get; set; }
    }

    public class HyperlinkDto
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }

    public class InsertDto
    {
        [JsonProperty("insert_id")]
        public string InsertId { get; set; }

        [JsonProperty("insert_text")]
        public string InsertText { get; set; }
    }

    public class NotaryInviteDto
    {
        [JsonProperty("notary_id")]
        public string NotaryId { get; set; }

        [JsonProperty("invite_status")]
        public string InviteStatus { get; set; }
    }

    public class OrganizationSettingDto
    {
        [JsonProperty("setting_name")]
        public string SettingName { get; set; }

        [JsonProperty("setting_value")]
        public string SettingValue { get; set; }
    }

    public class PaymentDto
    {
        [JsonProperty("payment_id")]
        public string PaymentId { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }
    }

    public class RadioButtonDto
    {
        [JsonProperty("radio_button_id")]
        public string RadioButtonId { get; set; }

        [JsonProperty("selected")]
        public bool Selected { get; set; }
    }

    public class SealDto
    {
        [JsonProperty("seal_id")]
        public string SealId { get; set; }

        [JsonProperty("seal_type")]
        public string SealType { get; set; }
    }

    public class SignatureDto
    {
        [JsonProperty("signature_id")]
        public string SignatureId { get; set; }

        [JsonProperty("signature_type")]
        public string SignatureType { get; set; }
    }

    public class RequestDto
    {
        [JsonProperty("request_id")]
        public string RequestId { get; set; }

        [JsonProperty("request_status")]
        public string RequestStatus { get; set; }
    }

    public class RoutingDetailDto
    {
        [JsonProperty("route_id")]
        public string RouteId { get; set; }

        [JsonProperty("route_order")]
        public int RouteOrder { get; set; }
    }

    public class TextDto
    {
        [JsonProperty("text_id")]
        public string TextId { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }

    public class PageDto
    {
        [JsonProperty("page_number")]
        public int PageNumber { get; set; }
    }

    public class LineDto
    {
        [JsonProperty("line_id")]
        public string LineId { get; set; }

        [JsonProperty("line_content")]
        public string LineContent { get; set; }
    }

    public class ShareInfoDto
    {
        [JsonProperty("shared_with")]
        public string SharedWith { get; set; }
    }
}
