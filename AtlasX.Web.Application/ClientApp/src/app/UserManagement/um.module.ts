import { CommonModule } from '@angular/common'
import { NgModule } from '@angular/core'
import { UserMRoutingModule } from './um-routing.module'
import { UserMComponent } from './um.component'

import { FormsModule, ReactiveFormsModule } from '@angular/forms'

import { ButtonModule } from 'primeng/button'
import { InputNumberModule } from 'primeng/inputnumber'
import { ToastModule } from 'primeng/toast'
import { RatingModule } from 'primeng/rating'
import { ConfirmDialogModule } from 'primeng/confirmdialog'
import { ConfirmDialog } from 'primeng/confirmdialog'

@NgModule({
  declarations: [UserMComponent],
  imports: [
    CommonModule,
    UserMRoutingModule,
    ButtonModule,
    InputNumberModule,
    FormsModule,
    ReactiveFormsModule,
    ToastModule,
    RatingModule,
    ConfirmDialogModule,
    // ConfirmDialog,
  ],
})
export class UserMModule {}
