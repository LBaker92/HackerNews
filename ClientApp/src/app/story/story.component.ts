import { Component, OnInit, ViewChild } from '@angular/core';
import { MatPaginator, PageEvent } from '@angular/material/paginator';
import { map, tap } from 'rxjs/operators';
import { StoryData } from '../models/story-data';
import { StoryService } from '../services/story.service';

@Component({
  selector: 'app-story',
  templateUrl: './story.component.html',
  styleUrls: ['./story.component.scss'],
})
export class StoryComponent implements OnInit {
  dataSource!: StoryData;
  displayedColumns = ['stories'];
  totalStories = 0;
  pageIndex = 0;
  pageSize = 10;

  searchText = '';

  @ViewChild(MatPaginator, { static: false }) paginator!: MatPaginator;

  isLoading = true;

  constructor(private storyService: StoryService) {}

  ngOnInit(): void {
    this.storyService
      .getStories(this.pageIndex, this.pageSize)
      .pipe(
        map((storyData: StoryData) => {
          this.dataSource = storyData;
          this.isLoading = false;
        })
      )
      .subscribe();
  }

  onSearchFieldChange(): void {
    this.isLoading = true;

    this.pageIndex = 0;
    this.paginator.pageIndex = 0;

    this.storyService
      .getStories(this.pageIndex, this.pageSize, this.searchText)
      .pipe(
        map((storyData: StoryData) => {
          this.dataSource = storyData;
          this.isLoading = false;
        })
      )
      .subscribe();
  }

  onPageChange(event: PageEvent): void {
    this.isLoading = true;

    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;

    this.storyService
      .getStories(event.pageIndex, event.pageSize, this.searchText)
      .pipe(
        map((storyData: StoryData) => {
          this.dataSource = storyData;
          this.isLoading = false;
        })
      )
      .subscribe();
  }
}
