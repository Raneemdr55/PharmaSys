import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { batchTask } from '../_models/batchTask';
import { checkedListTask } from '../_models/checkedListTask';
import { taskAssign } from '../_models/taskAssign';
import { rawMaterialsTask } from '../_models/rawMaterialsTask';
import { rangeSelectTask } from '../_models/rangeSelectTask';

@Injectable({
  providedIn: 'root'
})
export class BatchtaskService {

  baseUrl = environment.apiUrl;
  constructor(private http:HttpClient)
  { 

  }
  assign(taskassign:taskAssign) : Observable<boolean>
  {
      return this.http.put<boolean>(this.baseUrl+'Tasks/Assign',taskassign);
  }

  getBatchTask(id:number) :Observable<batchTask>
  {
    return this.http.get<batchTask> (this.baseUrl+'tasks/'+id);
  }

  getCheckedListTask(id:number) : Observable<checkedListTask>
  {
     return this.http.get<checkedListTask> (this.baseUrl+'tasks/checkedList/'+id);
  }
  getrawmaterialsTask(id:number) : Observable<rawMaterialsTask>
  {
     return this.http.get<rawMaterialsTask> (this.baseUrl+'tasks/rawmaterials/'+id);
  }
  getrangeSelectTask(id:number) : Observable<rangeSelectTask>
  {
     return this.http.get<rangeSelectTask> (this.baseUrl+'tasks/rangeSelect/'+id);
  }

}
