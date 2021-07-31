var map, infobox;

function GetMap() {
    map = new Microsoft.Maps.Map('#myMap', {
        credentials: 'AlRC594uOFvvT5iOTMQTikN1FMnXEzqIDu-Bftf_ZYKuOAMvtvcVmWfndKxLck4d',
        center: new Microsoft.Maps.Location(32.074031,34.792868),
        zoom: 9,
        showMapTypeSelector : false
    });

    //Create an infobox at the center of the map but don't show it.
    infobox = new Microsoft.Maps.Infobox(map.getCenter(), {
        visible: false
    });

    //Assign the infobox to a map instance.
    infobox.setMap(map);

    $.ajax({
        type: "GET",
        url: '/Branch/getBranches',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: successFunc,
        error: errorFunc
    })

    function errorFunc() {
        alert("oops, we have a problem!");
    }

    function successFunc(branches) {
        for (var i = 0; i < branches.length; i++) {
            var pushpin = new Microsoft.Maps.Pushpin(new Microsoft.Maps.Location(branches[i].locationX, branches[i].locationY),
                { width: 20, height: 20, visible: true, color: 'red' });


            pushpin.metadata = {
                title: branches[i].branchName,
                description: branches[i].addressInfo,
            }

            //Add a click event handler to the pushpin.
            Microsoft.Maps.Events.addHandler(pushpin, 'click', onClickShop);

            map.entities.push(pushpin);
        }
    }
}



function onClickShop(e) {
    //Make sure the infobox has metadata to display.
    if (e.target.metadata) {
        //Set the infobox options with the metadata of the pushpin.
        infobox.setOptions({
            location: e.target.getLocation(),
            title: e.target.metadata.title,
            description: e.target.metadata.description,
            visible: true
        });
    }
}