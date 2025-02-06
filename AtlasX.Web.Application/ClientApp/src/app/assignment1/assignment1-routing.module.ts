import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { Assignment1Component } from './assignment1.component'

const routes: Routes = [
  {
    path: '',
    component: Assignment1Component,
  },
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class Assignment1RoutingModule {}
