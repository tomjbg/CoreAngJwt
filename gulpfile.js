var gulp = require("gulp");
var cssmin = require("gulp-cssmin");
var less = require("gulp-less");
var concat = require("gulp-concat");
var uncss = require("gulp-uncss");


gulp.task("watch", function() {
    gulp.watch('**/*.less', ['less']);
});



gulp.task("less", function() {

    return gulp.src([
            "./src/**/*.less"
        ])
        .pipe(less())
        .pipe(concat("styles.css"))
        .pipe(cssmin())
        .pipe(uncss({
            html: ['**/*.html']
        }))
        .pipe(gulp.dest('./src'));

});



gulp.task("default", ['watch']);