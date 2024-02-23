import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject, filter, map, take } from 'rxjs';
import { BatchStates } from 'src/app/_enums/batchStates';
import { batch } from 'src/app/_models/batch';
import { batchIngredient } from 'src/app/_models/batchIngredient';
import { BarcodeService } from 'src/app/_services/barcode.service';
import { BatchService } from 'src/app/_services/batch.service';
import { UsersService } from 'src/app/_services/users.service';

@Component
({
  selector: 'app-batch-edit',
  templateUrl: './batch-edit.component.html',
  styleUrls: ['./batch-edit.component.css']
})

export class BatchEditComponent  implements OnInit
{
   
    
  displayedIngredientsColumns = ['ingredientName', 'qtyPerTube','qtyPerBatch']; 
   title:string;

   form : FormGroup;

   iseditmode:boolean;

    batch : batch;

    batchingredients : batchIngredient[];

    idparameter?: number;

    loggedUserId : string;


    constructor(private activatedRoute:ActivatedRoute,
      private router:Router,private batchservice:BatchService,
      private barcodeservice:BarcodeService,
      private userservice:UsersService)
    {

    }

    ngOnInit(): void
    {

         /*
        this.userservice.loggedUser$.subscribe( 
          ite =>
          {
            console.log(ite.username);
            this.loggedUserId=ite.id;
          }
        ,
        error=>
        {
          console.log(error);
          this.loggedUserId="";
        }
        ) ;

        */

        this.userservice.loggedUser$.pipe(
         take(1) 
        ).subscribe(
          res=> {
            console.log(res.username);
          this.loggedUserId=res.id;
          }
        )


        //#region  form group
        this.form =new FormGroup
        ( 
          {
          batchNO:new FormControl(''),
          batchSize:new FormControl(''),
          mFgDate:new FormControl(''),
          expDate:new FormControl(''),
          revision:new FormControl(''),
          revisionDate:new FormControl(''),
          barcode:new FormControl(''),
          mfno:new FormControl(''),
          ndcno:new FormControl(''),
          productId:new FormControl(''),
          productName:new FormControl(''),
          tubeWeight:new FormControl(''),
          tubesCount:new FormControl(''),
        }
        )
       
        //#endregion
        
      

        this.loadBatch();

    }

   
    
   
    
    loadBatch()
    {
     // this.idparameter = +this.activatedRoute.snapshot.params['id'];
      this.idparameter = this.activatedRoute.snapshot.params['id'];

      if (this.idparameter)
      {
        this.iseditmode=true;

        // load 
        this.batchservice.getBatch(this.idparameter).subscribe
        (
res=>
{
  this.batch = res;
  this.batchingredients = this.batch.batchIngredients; // remove later...no need
  this.title="Editing Batch No :"+this.batch.batchNO;
  this.form.patchValue( this.batch);
}
,error=>
{
console.log(error);
}
        )
      }
      else
      {
        this.iseditmode=false;
        this.title="Add a New Batch...";
        this.batch = {} as batch;
         this.form.patchValue(this.batch);
      }
    }


    onSubmit()
    {

       this.userservice.loggedUser$.pipe(
        take(1) 
       ).subscribe(
         res=> {
           console.log(res.username);
         this.loggedUserId=res.id;
         }
       )
      //#region  Getting The Batch Values from Form
    
      this.batch.batchNO  = this.form.get("batchNO")?.value;
      this.batch.batchSize  = +this.form.get("batchSize")?.value;
      this.batch.barcode  = this.form.get("barcode")?.value;
      this.batch.revision = this.form.get("revision")?.value;
      this.batch.revisionDate = this.form.get("revisionDate")?.value;
      this.batch.expDate = this.form.get("expDate")?.value;
      this.batch.batchStateId  = BatchStates.initialized; // replace with enumeration ... later
      this.batch.cartoonPictureURL  = "";
      this.batch.mfno  = this.form.get("mfno")?.value;
      this.batch.mFgDate  = this.form.get("mFgDate")?.value; // re- check for dates
      this.batch.ndcno  = this.form.get("ndcno")?.value;
      this.batch.productId  = +this.form.get("productId")?.value;
   //   this.batch.userId  = "0b17e502-1da0-45f4-80c9-6d104734a8dd"; // replace with logged user Id....
       this.batch.userId  =this.loggedUserId;
      this.batch.tubePictureURL  = "";
      this.batch.cartoonPictureURL  = "";
      this.batch.tubeWeight  = +this.form.get("tubeWeight")?.value;
      this.batch.tubesCount  = +this.form.get("tubesCount")?.value;
       //#endregion


       if (this.idparameter)
       {
         // update...
         this.batchservice.updateBatch(this.idparameter,this.batch).subscribe
         (
          res=>
          {
             console.log("Batch"+this.batch.batchNO+"has been updated");
             this.router.navigate(['/batches']);
          }
          ,error=>
          {
             console.log(error);
          }
         )
       }
       else
       {
          // insert....
          this.batchservice.addBatch(this.batch).subscribe(res=>
            {
              console.log("Batch " + this.batch.batchNO + " has been Added.");
              this.router.navigate(['/batches']);
            }
            ,error=>
            {
              console.log(error);
            }
            )
       }
    }




    ontubeWeightBlur(event:any)
    {
      this.calculateTubesCount();
    }

    onbatchSizeBlur(event:any)
    {
      this.calculateTubesCount();

    }
    onBarcodeBlur(event:any)
    {

       
  this.barcodeservice.getBarcode(event.target.value).subscribe
   (
  res=>
  {
    
     console.log(res);
     // do i need to reset the form value ?
     this.batch.productId = res.productId;
     this.batch.productName = res.productName;
     this.batch.ndcno = res.ndcno;
     this.batch.tubeWeight = res.tubeWeight;

     

     this.form.patchValue({
      productId:this.batch.productId,
      productName:this.batch.productName,
      ndcno:this.batch.ndcno,
      tubeWeight:this.batch.tubeWeight,
     // tubesCount:this.batch.tubesCount
     });

     // after that i need to set the tubes count ...

     this.calculateTubesCount();
     
  }

   ,error=>
   {
    console.log(error);
    this.batch.productId = 0;
    this.batch.productName="";
    this.batch.ndcno = "";
    this.batch.tubeWeight = 0;
    this.form.patchValue({
      productId:this.batch.productId,
      productName:this.batch.productName,
      ndcno:this.batch.ndcno,
      tubeWeight:this.batch.tubeWeight,
     })
   }
)

    

   
   }


   calculateTubesCount():void
   {
     
    let  batchsize :number  = +this.form.get("batchSize")?.value;
    let  tubewight :number = +this.form.get("tubeWeight")?.value;
   // let  batchsize = +this.batch.batchSize ;
   // let  tubewight =+ this.batch.tubeWeight;

    let    batchsizegrams = batchsize * 1000;
   // console.log(batchsizegrams);
    if (tubewight!=0)
    {
    let tubescount :number = 1;
   // tubescount =Number(batchsizegrams)  / Number(tubewight);

    tubescount = Math.ceil((batchsizegrams) / (tubewight)) ;

    this.batch.tubesCount =  +tubescount ;
    this.form.patchValue(
      {
        tubesCount:this.batch.tubesCount
      }
    )

    console.log(tubescount);
    }
     
   }




   SendBatch()
   {
    if (this.idparameter)
    {
      this.batchservice.sendBatch(this.idparameter).subscribe(res=>
        {
            if (res==true)
            {
              console.log("Batch Sended Successfully");
            }
            else
            {
              console.log("Batch Send Was Unsuccessfull");
            }
        }
        ,error=>
        {
          console.log(error);
        }
        )
    }
    else
    {
      console.log("No Batch Id Was Provided....");
    }
    
    
   }


/*
   showLoggedUserId()
   { 
    
    this.userservice.loggedUser$.subscribe
    (
      ite=>
      {
        console.log(ite.username);
      }
    ) ;
  }
  */
  





}
