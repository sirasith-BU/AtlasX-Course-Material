import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { LocatorComponent } from './locator.component'
import { QueryTaskComponent } from './querytask/query.component'
import { ClosetFacilityComponent } from './closet-facility/clo-fac.component'
import { RouteComponent } from './route/route.component'
import { SketchComponent } from './sketch/sketch.component'
import { LayerListComponent } from './layerList/layer.component'
import { SwipeComponent } from './swipe/swipe.component'

const routes: Routes = [
  {
    path: '',
    component: LocatorComponent,
  },
  {
    path: 'queryTask',
    component: QueryTaskComponent,
  },
  {
    path: 'closet-fac',
    component: ClosetFacilityComponent,
  },
  {
    path: 'sketch',
    component: SketchComponent,
  },
  {
    path: 'route',
    component: RouteComponent,
  },
  {
    path: 'layer',
    component: LayerListComponent,
  },
  {
    path: 'swipe',
    component: SwipeComponent,
  },
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class LocatorRoutingModule {}
