import { NgModule } from '@angular/core'
import { CommonModule } from '@angular/common'
import { Assignment1Component } from './assignment1.component'
import { Assignment1RoutingModule } from './assignment1-routing.module'
import { FormsModule } from '@angular/forms';

@NgModule({
  declarations: [Assignment1Component],
  imports: [CommonModule,Assignment1RoutingModule,FormsModule],
})
export class Assignment1Module {}
