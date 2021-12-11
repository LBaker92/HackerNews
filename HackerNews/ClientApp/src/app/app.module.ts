import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { StoryComponent } from './story/story.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatListModule } from '@angular/material/list';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator'; 
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { ErrorInterceptor } from './utilities/error-interceptor';
import { NotFoundComponent } from './not-found/not-found.component';

@NgModule({
  declarations: [AppComponent, StoryComponent, NotFoundComponent],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: 'stories', component: StoryComponent, pathMatch: 'full' },
      { path: '', redirectTo: 'stories', pathMatch: 'full' },
      { path: 'not-found', component: NotFoundComponent, pathMatch: 'full' },
      { path: '**', redirectTo: 'not-found', pathMatch: 'full' }
    ]),
    BrowserAnimationsModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatListModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatCardModule,
    MatInputModule,
    MatSnackBarModule
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ErrorInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
