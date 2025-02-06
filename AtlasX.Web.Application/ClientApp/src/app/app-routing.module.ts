import { NgModule } from '@angular/core'
import { Routes, RouterModule } from '@angular/router'

const routes: Routes = [
  {
    path: '',
    loadChildren: () => import('./home/home.module').then((m) => m.HomeModule),
  },
  {
    path: 'gis',
    loadChildren: () => import('./gis/gis.module').then((m) => m.GisModule),
    data: {
      systemId: 'GIS',
    },
  },
  {
    path: 'map',
    loadChildren: () => import('./map/map.module').then((m) => m.MapModule),
  },
  {
    path: 'locator',
    loadChildren: () => import('./locator/loactor.module').then((m) => m.LocatorModule),
  },
  {
    path: 'assignment1',
    loadChildren: () => import('./assignment1/assignment1.module').then((m) => m.Assignment1Module),
  },
  {
    path: 'um',
    loadChildren: () => import('./UserManagement/um.module').then((m) => m.UserMModule),
  },
]

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
