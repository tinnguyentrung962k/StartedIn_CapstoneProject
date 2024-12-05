using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StartedIn.CrossCutting.Constants;
using StartedIn.CrossCutting.DTOs.RequestDTO.RecruitInvite;
using StartedIn.CrossCutting.DTOs.ResponseDTO;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Recruitment;
using StartedIn.CrossCutting.DTOs.ResponseDTO.Transaction;
using StartedIn.CrossCutting.Enum;
using StartedIn.CrossCutting.Exceptions;
using StartedIn.Domain.Entities;
using StartedIn.Repository.Repositories.Interface;
using StartedIn.Service.Services.Interface;

namespace StartedIn.Service.Services;

public class RecruitmentService : IRecruitmentService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IAzureBlobService _azureBlobService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRecruitmentRepository _recruitmentRepository;
    private readonly IRecruitmentImageRepository _recruitmentImageRepository;
    private readonly ILogger<RecruitmentService> _logger;
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    public RecruitmentService(IProjectRepository projectRepository, IAzureBlobService azureBlobService, IUnitOfWork unitOfWork,
        IRecruitmentRepository recruitmentRepository, IRecruitmentImageRepository recruitmentImageRepository,
        ILogger<RecruitmentService> logger, IUserService userService, IMapper mapper)
    {
        _projectRepository = projectRepository;
        _azureBlobService = azureBlobService;
        _unitOfWork = unitOfWork;
        _recruitmentRepository = recruitmentRepository;
        _recruitmentImageRepository = recruitmentImageRepository;
        _logger = logger;
        _userService = userService;
        _mapper = mapper;
    }
    public async Task<Recruitment> CreateRecruitment(string projectId, string userId, CreateRecruitmentDTO createRecruitmentDto)
    {
        var project = await _projectRepository.QueryHelper().Filter(p => p.Id.Equals(projectId)).GetOneAsync();
        if (project == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
        }

        var recruitmentPost = await _recruitmentRepository.QueryHelper().Filter(r => r.ProjectId.Equals(projectId)).GetOneAsync();
        if (recruitmentPost != null)
        {
            throw new InvalidInputException(MessageConstant.RecruitmentPostExist);
        }

        var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
        var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
        if (projectRole != RoleInTeam.Leader)
        {
            throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
        }

        try
        {
            _unitOfWork.BeginTransaction();
            var recruitment = new Recruitment
            {
                ProjectId = projectId,
                Content = createRecruitmentDto.Content,
                Title = createRecruitmentDto.Title
            };
            var recruitmentEntity = _recruitmentRepository.Add(recruitment);
            foreach (var recruitFile in createRecruitmentDto.recruitFiles)
            {
                string url = await _azureBlobService.UploadRecruitmentImage(recruitFile);
                var recruitmentImg = new RecruitmentImg
                {
                    RecruitmentId = recruitmentEntity.Id,
                    FileName = Path.GetFileName(recruitFile.FileName),
                    ImageUrl = url
                };
                var recruitmentImgEntity = _recruitmentImageRepository.Add(recruitmentImg);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitAsync();
            return recruitmentEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating recruitment post.");
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    public async Task<Recruitment> GetRecruitmentPostById(string recruitmentId)
    {
        var recruitment = await _recruitmentRepository.GetRecruitmentPostById(recruitmentId);
        if (recruitment == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundRecruitmentPost);
        }

        return recruitment;
    }

    public async Task<Recruitment> UpdateRecruitment(string userId, string projectId, string recruitmentId,
        UpdateRecruitmentDTO updateRecruitmentDto)
    {
        var project = await _projectRepository.QueryHelper().Filter(p => p.Id.Equals(projectId)).GetOneAsync();
        if (project == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundProjectError);
        }
        
        var userInProject = await _userService.CheckIfUserInProject(userId, projectId);
        var projectRole = await _projectRepository.GetUserRoleInProject(userId, projectId);
        if (projectRole != RoleInTeam.Leader)
        {
            throw new UnauthorizedProjectRoleException(MessageConstant.RolePermissionError);
        }

        var recruitment = await _recruitmentRepository.GetRecruitmentPostById(recruitmentId);
        if (recruitment == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundRecruitmentPost);
        }

        try
        {
            recruitment.Title = updateRecruitmentDto.Title;
            recruitment.Content = updateRecruitmentDto.Content;
            recruitment.IsOpen = updateRecruitmentDto.IsOpen;
            _recruitmentRepository.Update(recruitment);
            await _unitOfWork.SaveChangesAsync();
            return recruitment;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            throw new Exception("Failed while update recruitment");
        }
    }

    public async Task<PaginationDTO<RecruitmentListDTO>> GetRecruitmentListWithLeader(int page, int size)
    {
        var recruitments = _recruitmentRepository.GetRecruitmentWithLeader();
        int totalCount =  await recruitments.CountAsync();
        var pagedResult = await recruitments
            .Skip((page - 1) * size)
            .Take(size).ToListAsync();
        var response = _mapper.Map<List<RecruitmentListDTO>>(pagedResult);
        var pagination = new PaginationDTO<RecruitmentListDTO>
        {
            Data = response,
            Page = page,
            Size = size,
            Total = totalCount

        };
        return pagination;
    }

    public async Task<Recruitment> GetRecruitmentPostInProject(string userId, string projectId)
    {
        var userInProject = await _userService.CheckIfUserInProject(userId, projectId);

        var recruitment = await _recruitmentRepository.QueryHelper().Include(r => r.RecruitmentImgs)
            .Filter(r => r.ProjectId.Equals(projectId)).GetOneAsync();

        if (recruitment == null)
        {
            throw new NotFoundException(MessageConstant.NotFoundRecruitmentPost);
        }

        return recruitment;
    }
}