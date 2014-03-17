//
//  Copyright (c) 2014, Masafumi Nishida. All rights reserved.
//  See https://github.com/masa1go/KSockets for details.
//

#import "KSIReceiver.h"

@interface KSFixedSizeMessageReceiver : NSObject <KSIReceiver>

- (id)initWithPayloadSize:(int)payloadSize;

@end
