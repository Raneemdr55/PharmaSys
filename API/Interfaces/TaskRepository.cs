using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOS;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace API.Interfaces
{
    public class TaskRepository : ITaskRepository
    {

        private readonly DataContext _dataContext;

        public TaskRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
            
        }
        public async Task<int> Add (BatchTask batchTask)
        {

            var result = _dataContext.BatchTasks.Add(batchTask);
            await _dataContext.SaveChangesAsync();
            return result.Entity.Id;
        }

        public async Task<BatchTask> getBatchTaskInfo(int Id)
        {
           return await _dataContext.BatchTasks.FirstOrDefaultAsync(x=>x.Id==Id);
        }

        public async Task<CheckedListTaskForViewDTO> getCheckedListTaskForView(int Id)
        {

            
            CheckedListTaskForViewDTO checkedListTaskForViewDTO=null;
            // first get the task info along with batch info and task type info 
           var BatchTaskDataSet=  await _dataContext.BatchTasks.AsNoTracking()
           .Include(x=>x.Department).
            Include(x=>x.TaskType)
            .ThenInclude(x=>x.TaskTypeCheckLists).AsNoTracking().
             FirstOrDefaultAsync(x=>x.Id==Id);

            if (BatchTaskDataSet!=null)
            {
                // mapping ... here done manually  ... 
                checkedListTaskForViewDTO = new CheckedListTaskForViewDTO();
                checkedListTaskForViewDTO.Id = BatchTaskDataSet.Id;
                checkedListTaskForViewDTO.BatchId = BatchTaskDataSet.BatchId;
                checkedListTaskForViewDTO.DepartmentId = BatchTaskDataSet.DepartmentId;
                checkedListTaskForViewDTO.DurationInSeconds = BatchTaskDataSet.DurationInSeconds;
                checkedListTaskForViewDTO.EndDate = BatchTaskDataSet.EndDate;
                checkedListTaskForViewDTO.StartDate =BatchTaskDataSet.StartDate;
                checkedListTaskForViewDTO.TaskTypeId =BatchTaskDataSet.TaskTypeId;
                checkedListTaskForViewDTO.UserId=BatchTaskDataSet.UserId;

                string departmentTitle=string.Empty;
                if (BatchTaskDataSet.Department!=null)
                {
                    departmentTitle = BatchTaskDataSet.Department.Title;
                }
                checkedListTaskForViewDTO.Title = string.Format("Task : {0} , Department : {1} ",
                  BatchTaskDataSet.TaskType.Title,departmentTitle
                );

                //  checkedListTaskForViewDTO.taskTypeCheckLists = BatchTaskDataSet.TaskType.TaskTypeCheckLists;

               
                checkedListTaskForViewDTO.taskTypeCheckLists = new List<TaskTypeCheckList>();
                foreach(TaskTypeCheckList taskTypeCheckListItem in BatchTaskDataSet.TaskType.TaskTypeCheckLists)
                {
                    TaskTypeCheckList taskTypeCheckListNewItem = new TaskTypeCheckList()
                    {
                        Id = taskTypeCheckListItem.Id,
                        TaskTypeId= taskTypeCheckListItem.TaskTypeId,
                        TaskTypeGroupId = taskTypeCheckListItem.TaskTypeGroupId,
                        Title = taskTypeCheckListItem.Title,
                        isChecked = taskTypeCheckListItem.isChecked
                    };
                    checkedListTaskForViewDTO.taskTypeCheckLists.Add(taskTypeCheckListNewItem);

                }
               
            }

            return checkedListTaskForViewDTO;


        }

        public async Task<RangeSelectTaskForViewDTO> GetRangeSelectTaskForViewDTO(int Id)
        {
           RangeSelectTaskForViewDTO rangeSelectTaskForViewDTO=null;

            // get the task info along with range data....

            var BatchTaskDataSet = await _dataContext.BatchTasks.AsNoTracking()
            .Include(x=>x.Department)
            .Include(x=>x.TaskType).ThenInclude(x=>x.taskTypeRanges).AsNoTracking()
            .Include(x=>x.TaskType).ThenInclude(x=>x.TaskTypeGroups).AsNoTracking()
            .FirstOrDefaultAsync(x=>x.Id==Id);

            if (BatchTaskDataSet!=null)
            {
                // mapping ... here done manually  ... 
                rangeSelectTaskForViewDTO = new RangeSelectTaskForViewDTO();
                rangeSelectTaskForViewDTO.Id = BatchTaskDataSet.Id;
                rangeSelectTaskForViewDTO.BatchId = BatchTaskDataSet.BatchId;
                rangeSelectTaskForViewDTO.DepartmentId = BatchTaskDataSet.DepartmentId;
                rangeSelectTaskForViewDTO.DurationInSeconds = BatchTaskDataSet.DurationInSeconds;
                rangeSelectTaskForViewDTO.EndDate = BatchTaskDataSet.EndDate;
                rangeSelectTaskForViewDTO.StartDate =BatchTaskDataSet.StartDate;
                rangeSelectTaskForViewDTO.TaskTypeId =BatchTaskDataSet.TaskTypeId;
                rangeSelectTaskForViewDTO.UserId=BatchTaskDataSet.UserId;

                string departmentTitle=string.Empty;
                if (BatchTaskDataSet.Department!=null)
                {
                    departmentTitle = BatchTaskDataSet.Department.Title;
                }
                rangeSelectTaskForViewDTO.Title = string.Format("Task : {0} , Department : {1} ",
                  BatchTaskDataSet.TaskType.Title,departmentTitle
                );

                rangeSelectTaskForViewDTO.taskTypeRangeDTOs = new List<TaskTypeRangeDTO>();
                foreach(TaskTypeRange TaskTypeRangeitem in BatchTaskDataSet.TaskType.taskTypeRanges)
                {
                    TaskTypeRangeDTO TaskTypeRangeDTONewItem = new TaskTypeRangeDTO();
                        TaskTypeRangeDTONewItem.Id = TaskTypeRangeitem.Id;
                        TaskTypeRangeDTONewItem. RangeValue =TaskTypeRangeitem.RangeValue;
                        TaskTypeRangeDTONewItem.TaskTypeGroupId = TaskTypeRangeitem.TaskTypeGroupId;
                        TaskTypeRangeDTONewItem.TaskTypeId = TaskTypeRangeitem.TaskTypeId;
                        TaskTypeRangeDTONewItem.TaskTypeGroupTitle = "";
                        if (BatchTaskDataSet.TaskType.TaskTypeGroups!=null)// not the best way to find the groupt Title...
                        {
                            if (TaskTypeRangeitem.TaskTypeGroupId!=null)
                            {
                                var groupItem= BatchTaskDataSet.TaskType.TaskTypeGroups.Where(x=>x.Id==TaskTypeRangeitem.TaskTypeGroupId).
                                 FirstOrDefault();
                                 if (groupItem!=null)
                                 {
                                     TaskTypeRangeDTONewItem.TaskTypeGroupTitle = groupItem.Title;
                                 }
                            }
                              
                        }

                    rangeSelectTaskForViewDTO.taskTypeRangeDTOs.Add(TaskTypeRangeDTONewItem);

                }
               
            }
           return   rangeSelectTaskForViewDTO;

        }

        public async Task<RawMaterialsTaskForViewDTO> GetRawMaterialsTaskForView(int Id)
        {
             RawMaterialsTaskForViewDTO rawMaterialsTaskForViewDTO=null;
             // first get the task info along with batch info and task type info 
              var BatchTaskDataSet=  await _dataContext.BatchTasks.AsNoTracking()
            .Include(x=>x.Department)
            .Include(x=>x.TaskType)
            .Include(x=>x.Batch).ThenInclude(x=>x.BatchIngredients).ThenInclude(x=>x.Ingredient).AsNoTracking().
             FirstOrDefaultAsync(x=>x.Id==Id);

                if (BatchTaskDataSet!=null)
                {
                        // mapping ... here done manually  ... 
                rawMaterialsTaskForViewDTO = new RawMaterialsTaskForViewDTO();
                rawMaterialsTaskForViewDTO.Id = BatchTaskDataSet.Id;
                rawMaterialsTaskForViewDTO.BatchId = BatchTaskDataSet.BatchId;
                rawMaterialsTaskForViewDTO.DepartmentId = BatchTaskDataSet.DepartmentId;
                rawMaterialsTaskForViewDTO.DurationInSeconds = BatchTaskDataSet.DurationInSeconds;
                rawMaterialsTaskForViewDTO.EndDate = BatchTaskDataSet.EndDate;
                rawMaterialsTaskForViewDTO.StartDate =BatchTaskDataSet.StartDate;
                rawMaterialsTaskForViewDTO.TaskTypeId =BatchTaskDataSet.TaskTypeId;
                rawMaterialsTaskForViewDTO.UserId=BatchTaskDataSet.UserId;

                string departmentTitle=string.Empty;
                if (BatchTaskDataSet.Department!=null)
                {
                    departmentTitle = BatchTaskDataSet.Department.Title;
                }
                rawMaterialsTaskForViewDTO.Title = string.Format("Task : {0} , Department : {1} ",
                  BatchTaskDataSet.TaskType.Title,departmentTitle
                );

                // mapping the ingredients.....(manually for now..avoiding injecting automapper here)
                 rawMaterialsTaskForViewDTO.batchIngredientDTOs =new List<BatchIngredientDTO>();
                 foreach(var item in BatchTaskDataSet.Batch.BatchIngredients)
                 {
                     BatchIngredientDTO batchIngredientNewItem =new BatchIngredientDTO ();
                     batchIngredientNewItem.Id=item.Id; // not necessary thuogh
                     batchIngredientNewItem.BatchId = item.BatchId;
                     batchIngredientNewItem.IngredientId = item.IngredientId;
                     batchIngredientNewItem.IngredientName = item.Ingredient.IngredientName;
                     batchIngredientNewItem.IsChecked=false;
                     batchIngredientNewItem.QTYPerBatch = item.QTYPerBatch;
                     batchIngredientNewItem.QTYPerTube = item.QTYPerTube;
                     rawMaterialsTaskForViewDTO.batchIngredientDTOs.Add(batchIngredientNewItem);
                 }

                }

             return rawMaterialsTaskForViewDTO;
        }

        public async Task<bool> SetAsAssigned(int TaskId,string UserId)
        {

            // use transaction .. later

            bool completed=false;
             using (IDbContextTransaction transaction=await _dataContext.Database.BeginTransactionAsync())
             {
                try 
                {
                var originalEntity = _dataContext.BatchTasks.FirstOrDefault(x=>x.Id==TaskId);
            if (originalEntity!=null)
            {
               // if (originalEntity.UserId==null) // the task is not assigned by  a user 
                {
                    // setting new properties for the original entity
                originalEntity.UserId = UserId;
                originalEntity.StartDate =DateTime.Now;
                originalEntity.TaskStateId = (int) Enumerations.TaskStatesEnum.processing;

                _dataContext.Entry(originalEntity).Property(x=>x.UserId).IsModified=true;
                _dataContext.Entry(originalEntity).Property(x=>x.StartDate).IsModified=true;
                _dataContext.Entry(originalEntity).Property(x=>x.TaskStateId).IsModified=true;


                // update also the notification-assigned by user property 
                // note : update all the notifications related the the task....
                var originalNotificationEntities = _dataContext.Notifications.Where(x=>x.BatchTaskId==TaskId).ToList();
                if (originalNotificationEntities!=null)
                {
                    foreach(var _item in originalNotificationEntities)
                    {
                     _item.AssignedByUserId=UserId;
                    _dataContext.Entry(_item).Property(x=>x.AssignedByUserId).IsModified=true;
                    
                    }
                  // we may want to push notifications here so other users can see that the user takes this specific task
                }
                await _dataContext.SaveChangesAsync();

                     await transaction.CommitAsync();
                   completed=true;
                }                   
            } 
                }
                catch(Exception)
             {
                 await transaction.RollbackAsync();
             }
            
             }
           
            return completed;
        }


         
         public async Task<bool> SetAsCompleted(int TaskId)
         {
            bool completed=false;

            var originalEntity = _dataContext.BatchTasks.FirstOrDefault(x=>x.Id==TaskId);
            if (originalEntity!=null)
            {
                // setting new properties for the original entity
                originalEntity.EndDate =DateTime.Now;
                originalEntity.TaskStateId = (int) Enumerations.TaskStatesEnum.finished;
                _dataContext.Entry(originalEntity).Property(x=>x.EndDate).IsModified=true;
                _dataContext.Entry(originalEntity).Property(x=>x.TaskStateId).IsModified=true;
                  await _dataContext.SaveChangesAsync();
                  completed=true;

            } 
            return completed;
         }

        
         public List<BatchTask> getBatchTasks(int _batchId)
         {
            List<BatchTask> batchTasks=new  List<BatchTask>();
            batchTasks = _dataContext.BatchTasks.Where(x=>x.BatchId==_batchId).ToList();
            return batchTasks;
         }

         public BatchTask GetBatchTask(int _batchId,int _taskIdtypeId,int _departmentId)
         {
            BatchTask batchTask= new BatchTask();
            batchTask= _dataContext.BatchTasks.Where(x=>x.TaskTypeId==_taskIdtypeId&&x.BatchId==
            _batchId&&x.DepartmentId==_departmentId).FirstOrDefault();
            return batchTask;
         }




         public async Task<IEnumerable<BatchTask>>GetUserRunningTasks(string userId)
         {
            return await _dataContext.BatchTasks.
             Include(x=>x.Batch).ThenInclude(x=>x.Product)
            .Include(x=>x.TaskType)
            .Where(x=>x.UserId==userId&&x.TaskStateId==(int)Enumerations.TaskStatesEnum.processing).ToListAsync();
         }



    }
}