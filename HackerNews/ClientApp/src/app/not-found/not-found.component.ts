import { Component, OnInit } from '@angular/core';
import { KittenService } from '../services/kitten.service';

@Component({
  selector: 'app-not-found',
  templateUrl: './not-found.component.html',
  styleUrls: ['./not-found.component.scss'],
})
export class NotFoundComponent implements OnInit {
  kittenImage: any;

  constructor(private kittenService: KittenService) {
    this.kittenService.getKitten().subscribe((kitten: Blob) => {
      this.createImageFromKittenBlob(kitten);
    });
  }

  ngOnInit(): void {}

  private createImageFromKittenBlob(kitten: Blob) {
    const fileReader = new FileReader();

    fileReader.addEventListener(
      'load',
      () => {
        this.kittenImage = fileReader.result;
      },
      false
    );

    if (kitten) {
      fileReader.readAsDataURL(kitten);
    }
  }
}
