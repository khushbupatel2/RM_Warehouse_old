document.head.innerHTML += `
  <style>
    .overlay-spinner {
            padding:0;
            margin:0;
            position: fixed;
            left: 0;
            top: 0;
            bottom: 0;
            right: 0;
            height:100%;
            background-color: #000;
            background-image: url(Blocks-1s-317px.svg);
            background-repeat: no-repeat;
            background-position: center;
            z-index: 9999;
            opacity: 0.8;
            filter: inherit;
        }
    }
</style>`


function spinner() {
    var overlay = '<div class="overlay-spinner"></div>';
    jQuery(overlay).appendTo('html');
}