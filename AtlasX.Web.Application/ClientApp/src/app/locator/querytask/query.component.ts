import { Component, OnInit, AfterViewInit, ViewChild, ElementRef } from '@angular/core'
import Graphic from '@arcgis/core/Graphic'
import FeatureLayer from '@arcgis/core/layers/FeatureLayer.js'
import SimpleFillSymbol from '@arcgis/core/symbols/SimpleFillSymbol'
import MapView from '@arcgis/core/views/MapView'
import Map from '@arcgis/core/Map'
import { UserService } from 'src/app/services/user.service'

@Component({
  selector: 'query-task',
  templateUrl: './query.component.html',
  styleUrls: ['./query.component.scss'],
})
export class QueryTaskComponent implements AfterViewInit, OnInit {
  @ViewChild('mapPanel', { static: true }) mapPanel!: ElementRef
  usaURL2 = 'https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/2'
  featureLayer: FeatureLayer
  states: any[] = []
  selectedStateIndex: number
  view: MapView

  constructor(private userService: UserService) {}

  ngOnInit(): void {
    this.featureLayer = new FeatureLayer({
      url: this.usaURL2,
    })
    const query = this.featureLayer.createQuery()
    query.where = '1=1'
    query.outFields = ['SUB_REGION', 'STATE_NAME', 'STATE_ABBR']
    query.returnGeometry = true

    this.featureLayer.queryFeatures(query).then((response) => {
      this.states = response.features.map((feature) => {
        return feature.attributes
      })
    })
  }

  ngAfterViewInit(): void {
    const map = new Map({
      basemap: 'topo-vector',
    })

    this.view = new MapView({
      container: this.mapPanel.nativeElement,
      map: map,
      center: [-99.36306483, 40.01512636],
      zoom: 5,
    })

    map.add(this.featureLayer)
  }

  showPolygon(polygon) {
    const marker = new SimpleFillSymbol({
      color: [255, 128, 0, 0.5], // Orange Opacity=0.5
      outline: {
        color: [255, 255, 255], // White
        width: 2, 
      },
    })

    const polygonGraphic = new Graphic({
      geometry: polygon,
      symbol: marker,
    })

    this.view.graphics.removeAll()
    this.view.graphics.add(polygonGraphic)

    // Zoom and Pan to Polygon
    this.view.goTo({
      target: polygon.extent.expand(1.5),
    })
  }

  onRowClick(state: any, index: number) {
    this.selectedStateIndex = index

    const query = this.featureLayer.createQuery()
    query.where = `STATE_NAME = '${state.state_name}'`
    query.returnGeometry = true

    this.featureLayer.queryFeatures(query).then((response) => {
      if (response.features.length > 0) {
        const geometry = response.features[0].geometry
        this.showPolygon(geometry)
      }
    })
  }
}
