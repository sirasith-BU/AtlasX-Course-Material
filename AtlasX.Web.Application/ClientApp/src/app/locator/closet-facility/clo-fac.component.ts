import { Component, Input, Output, EventEmitter, AfterViewInit, ViewChild, ElementRef } from '@angular/core'
import FeatureLayer from '@arcgis/core/layers/FeatureLayer'
import Map from '@arcgis/core/Map'
import MapView from '@arcgis/core/views/MapView'
import * as closestFacility from '@arcgis/core/rest/closestFacility'
import ClosestFacilityParameters from '@arcgis/core/rest/support/ClosestFacilityParameters'
import Graphic from '@arcgis/core/Graphic'
import * as geometryEngine from '@arcgis/core/geometry/geometryEngine.js'
import SimpleFillSymbol from '@arcgis/core/symbols/SimpleFillSymbol'
import SimpleMarkerSymbol from '@arcgis/core/symbols/SimpleMarkerSymbol'
import FeatureSet from '@arcgis/core/rest/support/FeatureSet'
import SimpleLineSymbol from '@arcgis/core/symbols/SimpleLineSymbol'

@Component({
  selector: 'closet-facility',
  templateUrl: './clo-fac.component.html',
  styleUrls: ['./clo-fac.component.scss'],
})
export class ClosetFacilityComponent implements AfterViewInit {
  // @Input() Routes: any // ค่าที่สามารถรับเข้ามา
  // @Output() selectedRoutes = new EventEmitter<any>() // ส่งออก
  @ViewChild('mapPanel', { static: true }) mapPanel!: ElementRef
  selectedCity: number
  cities: String[] = []
  view!: MapView
  Routes: any
  usaURL0 = 'https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/0'
  SanDiegoCF =
    'https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/ClosestFacility'
  usaFeatureLayer0 = new FeatureLayer({
    url: this.usaURL0,
  })

  // ngOnChanges() {
  //   if (this.Routes) {
  //     this.cities = []
  //     this.selectedCity = null
  //     this.Routes.routes.features.map((route) => {
  //       const facName = route.attributes.Name.split(' - ')[1]
  //       this.cities.push(facName)
  //     })
  //   }
  // }

  selectCity(index: number) {
    this.selectedCity = index
    const geometry = this.Routes.routes.features[index].geometry
    // this.selectedRoutes.emit(geometry)
    this.showSelectedRoutes(geometry)
  }

  ngAfterViewInit(): void {
    const map = new Map({
      basemap: 'topo-vector',
    })

    map.add(this.usaFeatureLayer0)

    this.view = new MapView({
      container: this.mapPanel.nativeElement,
      map: map,
      center: [-117.161087, 32.715736],
      zoom: 15,
    })

    this.view.when(() => {
      this.view.on('click', (event) => this.closetFacility(event)) // Closest Facility
    })
  }

  closetFacility = (event) => {
    let Buffer = this.addIncident(event.mapPoint)

    const query = this.usaFeatureLayer0.createQuery()
    query.geometry = Buffer
    query.spatialRelationship = 'intersects'
    query.returnGeometry = true
    query.outFields = ['*']

    this.usaFeatureLayer0.queryFeatures(query).then((result) => {
      const facility = result.features.map((feature) => {
        const marker = new SimpleMarkerSymbol({
          color: [255, 128, 0, 0.8],
          style: 'square',
          size: '10px',
          outline: {
            color: [255, 255, 255],
            width: 1,
          },
        })

        const pointGraphic = new Graphic({
          geometry: feature.geometry,
          symbol: marker,
          attributes: {
            name: feature.attributes.areaname,
          },
        })

        this.view.graphics.add(pointGraphic)
        return pointGraphic
      })

      const incident = new Graphic({
        geometry: event.mapPoint,
        symbol: new SimpleMarkerSymbol({
          color: [255, 128, 0],
          size: '12px',
          outline: {
            color: [255, 255, 255],
            width: 1,
          },
        }),
      })
      this.view.graphics.add(incident)

      closestFacility
        .solve(
          this.SanDiegoCF,
          new ClosestFacilityParameters({
            incidents: new FeatureSet({
              features: [incident],
            }),
            facilities: new FeatureSet({
              features: facility,
            }),
            returnRoutes: true,
            defaultTargetFacilityCount: 10,
          })
        )
        .then((response) => {
          this.Routes = response
          // console.log(this.Routes)
          response.routes.features.map((route) => {
            const routeGraphic = new Graphic({
              geometry: route.geometry,
              symbol: new SimpleLineSymbol({
                color: [0, 191, 255, 0.8],
                width: 3,
              }),
            })
            this.view.graphics.add(routeGraphic)
          })
          if (this.Routes) {
            this.cities = []
            this.selectedCity = null
            this.Routes.routes.features.map((route) => {
              const facName = route.attributes.Name.split(' - ')[1]
              this.cities.push(facName)
            })
          }
        })
        .catch((error) => {
          alert('Please start buffer only SanDiego')
          this.cities = []
          this.view.graphics.removeAll()
          this.view.goTo({
            center: [-117.161087, 32.715736],
            zoom: 10,
          })
        })
    })
  }

  addIncident(geometry) {
    // buffer 20 กิโลเมตร
    const ptBuff = geometryEngine.buffer(geometry, 20, 'kilometers')

    // ตรวจสอบว่า ptBuff เป็น Array หรือไม่
    const Buffer = Array.isArray(ptBuff) ? ptBuff[0] : ptBuff

    const marker = new SimpleFillSymbol({
      color: [255, 128, 0, 0.5], // Orange
      outline: {
        color: [255, 255, 255], // White
        width: 1,
      },
    })

    const bufferGraphic = new Graphic({
      geometry: Buffer, // ใช้ geometry ของ buffer
      symbol: marker,
    })

    this.view.graphics.removeAll()
    this.view.graphics.add(bufferGraphic)

    // Zoom and Pan to Polygon
    this.view.goTo({
      target: Buffer.extent.expand(1),
    })
    return Buffer
  }

  private routeGraphic: Graphic
  showSelectedRoutes(geometry) {
    // ลบเส้นก่อนหน้า
    if (this.routeGraphic) {
      this.view.graphics.remove(this.routeGraphic)
    }
    this.routeGraphic = new Graphic({
      geometry: geometry,
      symbol: new SimpleLineSymbol({
        color: [0, 0, 0],
        width: 3,
      }),
    })

    this.view.graphics.add(this.routeGraphic)

    // Zoom and Pan to Polygon
    this.view.goTo({
      target: geometry.extent.expand(1.5),
    })
  }
}
